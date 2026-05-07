using UnityEngine;

[System.Serializable]
public class AnvilUpgradeLevel
{
    public float slamForceIncrement;
    public float hangTimeIncrement;
    public int cost;
}

[CreateAssetMenu(fileName = "AnvilAbility", menuName = "Abilities/Anvil Ability")]
public class AnvilAbility : AbilityBase
{
    [Header("Anvil Settings")]
    public float baseSlamForce = 50f;
    public float baseHangTime  = 0.5f;

    [Header("Upgrades")]
    public AnvilUpgradeLevel[] upgradeLevels;

    public override bool IsSingleUse       => true;
    public override int  MaxUpgradeLevels  => upgradeLevels?.Length ?? 0;

    public override int GetUpgradeCost(int currentLevel)
    {
        if (upgradeLevels == null || currentLevel >= upgradeLevels.Length)
            return int.MaxValue;
        return upgradeLevels[currentLevel].cost;
    }

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float totalForce    = 0f;
        float totalHangTime = 0f;

        for (int i = 0; i < totalLevel && i < upgradeLevels.Length; i++)
        {
            totalForce    += upgradeLevels[i].slamForceIncrement;
            totalHangTime += upgradeLevels[i].hangTimeIncrement;
        }

        // Reusing totalDelay slot for hang time — anvil doesn't use explosion fields
        controller.SetAbilityUpgrades(totalForce, 0f, 0f, 0f, totalHangTime);
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        var slam = user.GetComponent<AnvilSlamController>();
        if (slam == null) return;

        var ac       = user.GetComponent<AbilityController>();
        float hang   = baseHangTime  + (ac?.DelayReduction ?? 0f);
        float force  = baseSlamForce + upgradeBoost;

        slam.StartSlam(hang, force);
    }
}