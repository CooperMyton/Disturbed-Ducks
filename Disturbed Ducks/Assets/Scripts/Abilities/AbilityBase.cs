using UnityEngine;

/// Abstract base for all duck abilities.
/// To add a new ability: create a new class that inherits from this,
/// implement all abstract members, and create an asset via CreateAssetMenu.

public abstract class AbilityBase : ScriptableObject
{
    [Header("Ability Info")]
    public string abilityName = "Ability";
    public float  cooldown    = 3f;

    public virtual bool IsSingleUse => false;

    /// Total number of upgrade levels available (level 1 = unlock).
    public abstract int MaxUpgradeLevels { get; }

    /// Cost to advance from currentLevel to currentLevel + 1.
    public abstract int GetUpgradeCost(int currentLevel);

    /// Resets and reapplies all upgrade effects for the given total purchased level.
    /// Called by UpgradeManager on duck switch and after each upgrade purchase.
    /// Each subclass calls controller.SetAbilityUpgrades with its own totals.
    public abstract void ApplyAllUpgrades(int totalLevel, AbilityController controller);

    /// Called by AbilityController when the player presses the ability key.
    /// user is the duck's root GameObject — grab whatever components you need.
    public abstract void Use(GameObject user, float upgradeBoost);
}