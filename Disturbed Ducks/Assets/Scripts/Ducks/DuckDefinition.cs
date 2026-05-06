using UnityEngine;

/// Single ScriptableObject that defines everything about a duck type.
/// To add a new duck: duplicate this asset and change the values — no code needed.

[CreateAssetMenu(fileName = "NewDuck", menuName = "Ducks/Duck Definition")]
public class DuckDefinition : ScriptableObject
{
    [Header("Identity")]
    public string duckName = "Basic Duck";

    [Header("Purchase")]
    [Tooltip("Cost for each successive purchase of this duck. " +
             "Index 0 = first duck, index 1 = second, etc. " +
             "Last entry repeats if the player buys more than the array length.")]
    public int[] purchaseCosts = new int[] { 50 };
    [Tooltip("Maximum number of this duck type the player can own at once")]
    public int maxOwned = 3;

    [Header("Base Stats")]
    [Tooltip("Hard speed cap before upgrades")]
    public float baseMaxSpeed    = 35f;
    [Tooltip("Degrees per second pitch/yaw — manoeuvrability base")]
    public float baseTurnSpeed   = 70f;
    public float baseGlideGravity = 12f;
    public float baseMinSpeed    = 5f;

    [Header("Models — drag prefabs here when ready")]
    public GameObject neutralModel;
    public GameObject flightModel;
    public GameObject crashedModel;

    [Header("Sounds — drag AudioClips here when ready")]
    public AudioClip launchSound;
    public AudioClip crashSound;
    public AudioClip abilitySound;

    [Header("Ability")]
    public AbilityBase ability;

    [Header("Explosion — Bomb Duck only, leave null for Basic Duck")]
    public ExplosionDefinition explosionDefinition;

    [Header("Upgrades")]
    public StatUpgradeTrack maxSpeedUpgrade = new StatUpgradeTrack
    {
        upgradeName = "Max Speed"
    };
    public StatUpgradeTrack manoeuvrabilityUpgrade = new StatUpgradeTrack
    {
        upgradeName = "Manoeuvrability"
    };
    // Ability upgrade data (costs, increments) now lives on the AbilityBase asset
    // so each ability only exposes fields relevant to it.

    /// Returns the purchase cost for the nth duck of this type.
    public int GetPurchaseCost(int currentlyOwned)
    {
        if (purchaseCosts == null || purchaseCosts.Length == 0) return 0;
        int index = Mathf.Clamp(currentlyOwned, 0, purchaseCosts.Length - 1);
        return purchaseCosts[index];
    }
}

// -------------------------------------------------------------------------

[System.Serializable]
public class StatUpgradeLevelData
{
    [Tooltip("How much the stat increases at this level")]
    public float statIncrement = 5f;
    [Tooltip("Cost to purchase this level")]
    public int cost = 0;
}

[System.Serializable]
public class StatUpgradeTrack
{
    public string upgradeName = "Upgrade";
    public StatUpgradeLevelData[] levels = new StatUpgradeLevelData[10];
}