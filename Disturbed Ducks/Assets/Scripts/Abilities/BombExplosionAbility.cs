using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BombUpgradeLevel
{
    [Tooltip("Explosion radius increase at this level")]
    public float radiusIncrement = 0.5f;
    [Tooltip("Explosion damage increase at this level")]
    public float damageIncrement = 10f;
    [Tooltip("Seconds removed from detonation delay at this level")]
    public float delayReduction  = 0.1f;
    [Tooltip("Currency cost to purchase this level")]
    public int   cost            = 75;
}

[CreateAssetMenu(fileName = "BombExplosionAbility",
                 menuName  = "Abilities/Bomb Explosion Ability")]
public class BombExplosionAbility : AbilityBase
{
    [Header("Upgrades")]
    [Tooltip("Level 1 = unlock. Levels 2+ improve radius, damage, and delay.")]
    public BombUpgradeLevel[] upgradeLevels = new BombUpgradeLevel[10];

    public override bool IsSingleUse       => true;
    public override int  MaxUpgradeLevels  => upgradeLevels.Length;

    public override int GetUpgradeCost(int currentLevel)
        => currentLevel < upgradeLevels.Length ? upgradeLevels[currentLevel].cost : 0;

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float tRadius = 0f, tDamage = 0f, tDelay = 0f;
        // Level 1 = unlock only — improvements start at level 2
        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
        {
            tRadius += upgradeLevels[i].radiusIncrement;
            tDamage += upgradeLevels[i].damageIncrement;
            tDelay  += upgradeLevels[i].delayReduction;
        }
        controller.SetAbilityUpgrades(0f, 0f, tRadius, tDamage, tDelay);
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        var explosion = user.GetComponent<ExplosionOnCrash>();
        if (explosion == null)
        {
            Debug.LogWarning("BombExplosionAbility: ExplosionOnCrash not found on duck");
            return;
        }
        var controller = user.GetComponent<AbilityController>();
        explosion.StartAbilityCountdown(
            controller?.RadiusBoost    ?? 0f,
            controller?.DamageBoost    ?? 0f,
            controller?.DelayReduction ?? 0f);
    }
    public override string GetUpgradePreview(int currentLevel)
    {
        if (currentLevel == 0 || currentLevel >= upgradeLevels.Length) return string.Empty;
        var level = upgradeLevels[currentLevel];
        var parts = new List<string>();
        if (level.radiusIncrement > 0) parts.Add($"+{level.radiusIncrement:F1} Radius");
        if (level.damageIncrement > 0) parts.Add($"+{level.damageIncrement:F0} Damage");
        if (level.delayReduction  > 0) parts.Add($"-{level.delayReduction:F1}s Fuse");
        return string.Join(", ", parts);
    }

    public override List<(string, string)> GetCurrentStats(DuckDefinition def, int abilityLevel)
    {
        float totalRadius = 0f, totalDamage = 0f, totalDelay = 0f;
        for (int i = 1; i < abilityLevel && i < upgradeLevels.Length; i++)
        {
            totalRadius += upgradeLevels[i].radiusIncrement;
            totalDamage += upgradeLevels[i].damageIncrement;
            totalDelay  += upgradeLevels[i].delayReduction;
        }

        var result = new List<(string, string)>();
        if (def.explosionDefinition != null)
        {
            float radius = def.explosionDefinition.abilityRadius + totalRadius;
            float damage = def.explosionDefinition.abilityDamage + totalDamage;
            float fuse   = Mathf.Max(0f, def.explosionDefinition.abilityDelay - totalDelay);
            result.Add(("Blast Radius", $"{radius:F1}"));
            result.Add(("Blast Damage", $"{damage:F0}"));
            result.Add(("Fuse Length",  $"{fuse:F1}s"));
        }
        return result;
    }
    }