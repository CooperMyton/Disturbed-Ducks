using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileDuckUpgradeLevel
{
    public int ammoIncrement = 1;
    public float damageIncrement = 8f;
    public int cost = 100;
}

[CreateAssetMenu(fileName = "ProjectileDuckAbility", menuName = "Abilities/Projectile Duck Ability")]
public class ProjectileDuckAbility : AbilityBase
{
    [Header("Projectile Settings")]
    public int baseAmmo = 3;
    public float baseDamage = 40f;
    public float projectileSpeed = 50f;
    public GameObject projectilePrefab;

    [Header("Upgrades")]
    public ProjectileDuckUpgradeLevel[] upgradeLevels = new ProjectileDuckUpgradeLevel[5];

    public override bool IsSingleUse => false;
    public override bool UsesCustomStatusText => true;
    public override int MaxUpgradeLevels => upgradeLevels?.Length ?? 0;

    public override int GetUpgradeCost(int currentLevel)
    {
        if (upgradeLevels == null || currentLevel >= upgradeLevels.Length)
            return 0;

        return upgradeLevels[currentLevel].cost;
    }

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        int totalAmmoBonus = 0;
        float totalDamageBonus = 0f;

        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
        {
            totalAmmoBonus += upgradeLevels[i].ammoIncrement;
            totalDamageBonus += upgradeLevels[i].damageIncrement;
        }

        controller.SetAbilityUpgrades(totalDamageBonus, 0f, totalAmmoBonus, 0f, 0f);

        var shooter = controller.GetComponent<ProjectileDuckShooter>();
        if (shooter != null)
        {
            shooter.Configure(
                projectilePrefab,
                baseAmmo + totalAmmoBonus,
                baseDamage + totalDamageBonus,
                projectileSpeed
            );
        }
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        var shooter = user.GetComponent<ProjectileDuckShooter>();
        if (shooter == null) return;

        shooter.TryShoot();
    }

    public override string GetUpgradePreview(int currentLevel)
    {
        if (currentLevel == 0 || upgradeLevels == null || currentLevel >= upgradeLevels.Length)
            return string.Empty;

        var level = upgradeLevels[currentLevel];
        var parts = new List<string>();

        if (level.ammoIncrement > 0)
            parts.Add($"+{level.ammoIncrement} Ammo");

        if (level.damageIncrement > 0f)
            parts.Add($"+{level.damageIncrement:F0} Damage");

        return string.Join(", ", parts);
    }

    public override List<(string, string)> GetCurrentStats(DuckDefinition def, int abilityLevel)
    {
        int ammo = baseAmmo;
        float damage = baseDamage;

        for (int i = 1; i < abilityLevel && i < upgradeLevels.Length; i++)
        {
            ammo += upgradeLevels[i].ammoIncrement;
            damage += upgradeLevels[i].damageIncrement;
        }

        return new List<(string, string)>
        {
            ("Ammo", $"{ammo}"),
            ("Bullet Damage", $"{damage:F0}"),
            ("Bullet Speed", $"{projectileSpeed:F0}")
        };
    }
}