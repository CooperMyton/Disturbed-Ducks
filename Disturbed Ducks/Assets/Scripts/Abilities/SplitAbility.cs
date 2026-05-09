using UnityEngine;

[System.Serializable]
public class SplitUpgradeLevel
{
    public float speedIncrement = 2f;
    public float massIncrement  = 0.1f;
    public int   cost           = 75;
}

[CreateAssetMenu(fileName = "SplitAbility", menuName = "Abilities/Split Ability")]
public class SplitAbility : AbilityBase
{
    [Header("Split Settings")]
    public float baseSpeed  = 15f;
    public float splitAngle = 25f;
    public float baseMass   = 0.5f;

    [Header("Upgrades")]
    public SplitUpgradeLevel[] upgradeLevels;

    [Tooltip("Total upgrade level at which the 3rd mini duck unlocks")]
    public int extraDuckUnlockLevel = 6;

    public override bool IsSingleUse      => true;
    public override int  MaxUpgradeLevels => upgradeLevels?.Length ?? 0;

    public override int GetUpgradeCost(int currentLevel)
        => currentLevel < upgradeLevels.Length ? upgradeLevels[currentLevel].cost : 0;

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float totalSpeed = 0f;
        float totalMass  = 0f;

        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
        {
            totalSpeed += upgradeLevels[i].speedIncrement;
            totalMass  += upgradeLevels[i].massIncrement;
        }

        // upgradeBoost = speed, damageBoost = mass, radiusBoost = extra duck flag
        float extraDuckFlag = totalLevel >= extraDuckUnlockLevel ? 1f : 0f;
        controller.SetAbilityUpgrades(totalSpeed, 0f, extraDuckFlag, totalMass, 0f);
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        var split = user.GetComponent<SplitController>();
        if (split == null) return;

        var ac    = user.GetComponent<AbilityController>();
        float speed = baseSpeed + upgradeBoost;
        float mass  = baseMass  + (ac?.DamageBoost  ?? 0f);
        int   count = (ac?.RadiusBoost ?? 0f) >= 1f ? 3 : 2;

        split.Split(speed, mass, count, splitAngle);
    }
}