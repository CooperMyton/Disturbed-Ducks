using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostPhaseUpgradeLevel
{
    public float durationIncrement = 0.5f;
    public int cost = 100;
}

[CreateAssetMenu(fileName = "GhostPhaseAbility", menuName = "Abilities/Ghost Phase Ability")]
public class GhostPhaseAbility : AbilityBase
{
    [Header("Phase Settings")]
    public float baseDuration = 1.5f;

    [Header("Upgrades")]
    public GhostPhaseUpgradeLevel[] upgradeLevels = new GhostPhaseUpgradeLevel[5];

    private float _remainingDuration;

    public override bool UsesHeldInput => true;
    public override bool IsSingleUse => true;
    public override int MaxUpgradeLevels => upgradeLevels.Length;

    public override int GetUpgradeCost(int currentLevel)
        => currentLevel < upgradeLevels.Length ? upgradeLevels[currentLevel].cost : 0;

    public override void ApplyAllUpgrades(int totalLevel, AbilityController controller)
    {
        float totalDurationBonus = 0f;

        for (int i = 1; i < totalLevel && i < upgradeLevels.Length; i++)
            totalDurationBonus += upgradeLevels[i].durationIncrement;

        controller.SetAbilityUpgrades(totalDurationBonus, 0f);
    }

    public override void Use(GameObject user, float upgradeBoost)
    {
        // Held-input ability uses OnHeldStarted/OnHeld/OnHeldEnded.
    }

    public override void OnHeldStarted(GameObject user, float upgradeBoost)
    {
        _remainingDuration = baseDuration + upgradeBoost;
        user.GetComponent<GhostPhaseController>()?.BeginPhase();
        AbilityUI.Instance?.OnPhaseStarted(_remainingDuration);

    }

    public override void OnHeld(GameObject user, float upgradeBoost)
    {
        _remainingDuration -= Time.deltaTime;

        if (_remainingDuration <= 0f)
            user.GetComponent<AbilityController>()?.EndHeldAbility();
    }

    public override void OnHeldEnded(GameObject user, float upgradeBoost)
    {
        user.GetComponent<GhostPhaseController>()?.EndPhase();
    }

    public override string GetUpgradePreview(int currentLevel)
    {
        if (currentLevel == 0 || currentLevel >= upgradeLevels.Length) return string.Empty;
        return $"+{upgradeLevels[currentLevel].durationIncrement:F1}s Phase";
    }

    public override List<(string, string)> GetCurrentStats(DuckDefinition def, int abilityLevel)
    {
        float duration = baseDuration;

        for (int i = 1; i < abilityLevel && i < upgradeLevels.Length; i++)
            duration += upgradeLevels[i].durationIncrement;

        return new List<(string, string)>
        {
            ("Phase Duration", $"{duration:F1}s")
        };
    }
}
