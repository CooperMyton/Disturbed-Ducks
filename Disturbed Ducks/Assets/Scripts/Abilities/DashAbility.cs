using UnityEngine;

[System.Serializable]
public class DashUpgradeLevel
{
    [Tooltip("Flat speed added to dash at this level")]
    public float speedBoostIncrement = 2f;
    [Tooltip("Seconds removed from cooldown at this level")]
    public float cooldownReduction = 0.2f;
    [Tooltip("Currency cost to purchase this level")]
    public int cost = 50;
}

[CreateAssetMenu(fileName = "DashAbility", menuName = "Abilities/Dash Ability")]
public class DashAbility : AbilityBase
{
    [Header("Dash Settings")]
    [Tooltip("Flat speed added on dash before upgrade boosts")]
    public float baseSpeedBoost = 15f;

    [Header("Upgrades")]
    [Tooltip("Level 1 = unlock. Levels 2+ add speed and cooldown improvements.")]
    public DashUpgradeLevel[] upgradeLevels = new DashUpgradeLevel[10];

    public override int  MaxUpgradeLevels => upgradeLevels.Length;

    public override int GetUpgradeCost(int currentLevel)
        => currentLevel < upgradeLevels.Length ? upgradeLevels[currentLevel].cost : 0;

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float tBoost = 0f, tCooldown = 0f;
        // Level 1 = unlock only — no stat increment at that level
        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
        {
            tBoost    += upgradeLevels[i].speedBoostIncrement;
            tCooldown += upgradeLevels[i].cooldownReduction;
        }
        controller.SetAbilityUpgrades(tBoost, tCooldown);
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        var flight = user.GetComponent<DuckFlightController>();
        if (flight == null) return;
        flight.ApplySpeedBoost(baseSpeedBoost + upgradeBoost);
    }

    public override bool IsSingleUse => true;
}