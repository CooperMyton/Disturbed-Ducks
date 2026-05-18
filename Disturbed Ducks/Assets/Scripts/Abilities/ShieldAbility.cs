using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShieldUpgradeLevel
{
    public float durationIncrement = 0.5f;
    public float damageMultiplierIncrement = 0.1f;
    public int cost = 100;
}

[CreateAssetMenu(fileName = "ShieldAbility", menuName = "Abilities/Shield Ability")]
public class ShieldAbility : AbilityBase
{
    [Header("Shield Settings")]
    public float baseDuration = 2f;
    public float baseDamageMultiplier = 1.25f;

    [Header("Upgrades")]
    public ShieldUpgradeLevel[] upgradeLevels = new ShieldUpgradeLevel[5];

    public override bool IsSingleUse => true;
    public override int MaxUpgradeLevels => upgradeLevels.Length;

    public override int GetUpgradeCost(int currentLevel)
        => currentLevel < upgradeLevels.Length ? upgradeLevels[currentLevel].cost : 0;

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float durationBonus = 0f;
        float damageBonus = 0f;

        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
        {
            durationBonus += upgradeLevels[i].durationIncrement;
            damageBonus += upgradeLevels[i].damageMultiplierIncrement;
        }

        controller.SetAbilityUpgrades(durationBonus, 0f, 0f, damageBonus);
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        var ability = user.GetComponent<AbilityController>();
        var shield = user.GetComponent<ShieldController>();
        if (shield == null) return;

        float duration = baseDuration + upgradeBoost;
        float damageMultiplier = baseDamageMultiplier + (ability != null ? ability.DamageBoost : 0f);

        shield.Activate(duration, damageMultiplier);
        AbilityUI.Instance?.OnPhaseStarted(duration);
    }

    public override string GetUpgradePreview(int currentLevel)
    {
        if (currentLevel == 0 || currentLevel >= upgradeLevels.Length) return string.Empty;

        var level = upgradeLevels[currentLevel];
        var parts = new List<string>();

        if (level.durationIncrement > 0f)
            parts.Add($"+{level.durationIncrement:F1}s Shield");
        if (level.damageMultiplierIncrement > 0f)
            parts.Add($"+{level.damageMultiplierIncrement:F2}x Damage");

        return string.Join(", ", parts);
    }

    public override List<(string, string)> GetCurrentStats(DuckDefinition def, int abilityLevel)
    {
        float duration = baseDuration;
        float damageMultiplier = baseDamageMultiplier;

        for (int i = 1; i < abilityLevel && i < upgradeLevels.Length; i++)
        {
            duration += upgradeLevels[i].durationIncrement;
            damageMultiplier += upgradeLevels[i].damageMultiplierIncrement;
        }

        return new List<(string, string)>
        {
            ("Shield Duration", $"{duration:F1}s"),
            ("Damage Multiplier", $"{damageMultiplier:F2}x")
        };
    }
}
