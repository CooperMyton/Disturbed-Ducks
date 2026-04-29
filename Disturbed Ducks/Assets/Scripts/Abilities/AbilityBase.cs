using UnityEngine;


/// Abstract base for all duck abilities.
/// To add a new ability: create a new class that inherits from this,
/// implement Use(), and create an asset via the CreateAssetMenu.

public abstract class AbilityBase : ScriptableObject
{
    [Header("Ability Info")]
    public string abilityName = "Ability";
    public float cooldown = 3f;

    /// <summary>
    /// Upgrade boost is accumulated boost from ability upgrade levels
    /// Called by AbilityController when the player presses the ability key.
    /// user is the duck's GameObject — grab whatever components you need from it.
    /// </summary>
    public abstract void Use(GameObject user, float upgradeBoost);
}