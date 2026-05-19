using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DuckFamilyUpgradeLevel
{
    public float massIncrement = 0.15f;
    public int cost = 100;
}

[CreateAssetMenu(fileName = "DuckFamilyAbility", menuName = "Abilities/Duck Family Ability")]
public class DuckFamilyAbility : AbilityBase
{
    [Header("Family Settings")]
    public int startingFamilyCount = 3; // includes leader
    public float familyDuckMass = 0.5f;
    public float vAngle = 35f;
    public float spacing = 1.1f;

    [Header("Upgrades")]
    public DuckFamilyUpgradeLevel[] upgradeLevels = new DuckFamilyUpgradeLevel[5];

    public override bool IsSingleUse => true;
    public override int MaxUpgradeLevels => upgradeLevels?.Length ?? 0;

    public override int GetUpgradeCost(int currentLevel)
        => upgradeLevels != null && currentLevel < upgradeLevels.Length ? upgradeLevels[currentLevel].cost : 0;

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float massBonus = 0f;
        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
            massBonus += upgradeLevels[i].massIncrement;

        controller.SetAbilityUpgrades(0f, 0f, Mathf.Max(0, totalLevel), massBonus);
    }

    public override void Use(GameObject user, float upgradeBoost) { }

    public override string GetUpgradePreview(int currentLevel)
    {
        if (upgradeLevels == null || currentLevel >= upgradeLevels.Length) return string.Empty;
        return $"+1 Duck, +{upgradeLevels[currentLevel].massIncrement:F2} Family Mass";
    }

    public override List<(string, string)> GetCurrentStats(DuckDefinition def, int abilityLevel)
    {
        int count = startingFamilyCount + Mathf.Max(0, abilityLevel);
        float mass = familyDuckMass;

        for (int i = 1; i < abilityLevel && i < upgradeLevels.Length; i++)
            mass += upgradeLevels[i].massIncrement;

        return new List<(string, string)>
        {
            ("Family Size", $"{count}"),
            ("Family Duck Mass", $"{mass:F2}"),
            ("V Angle", $"{vAngle:F0} deg")
        };
    }
}