using UnityEngine;


/// Single ScriptableObject that defines everything about a duck type.
/// To add a new duck: duplicate this asset and change the values — no code needed.

[CreateAssetMenu(fileName = "NewDuck", menuName = "Ducks/Duck Definition")]
public class DuckDefinition : ScriptableObject
{
    [Header("Identity")]
    public string duckName = "Basic Duck";

    [Header("Purchase")]
    [Tooltip("Cost to buy the first duck of this type")]
    public int basePurchaseCost = 50;
    [Tooltip("Each additional duck of this type costs this much more")]
    public float purchaseCostMultiplier = 1.5f;


    [Header("Base Stats")]
    [Tooltip("Hard speed cap before upgrades")]
    public float baseMaxSpeed = 35f;
    [Tooltip("Degrees per second pitch/yaw — manoeuvrability base")]
    public float baseTurnSpeed = 70f;
    public float baseGlideGravity = 12f;
    public float baseMinSpeed = 5f;

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

    [Header("Upgrades")]
    public StatUpgradeTrack maxSpeedUpgrade = new StatUpgradeTrack
    {
        upgradeName = "Max Speed",
        maxLevels = 10,
        costPerLevel = 0,
        incrementPerLevel = 5f
    };

    public StatUpgradeTrack manoeuvrabilityUpgrade = new StatUpgradeTrack
    {
        upgradeName = "Manoeuvrability",
        maxLevels = 10,
        costPerLevel = 0,
        incrementPerLevel = 8f
    };

    public AbilityUpgradeTrack abilityUpgrade = new AbilityUpgradeTrack();

    public int GetPurchaseCost(int currentlyOwned)
    {
        return Mathf.RoundToInt(basePurchaseCost * Mathf.Pow(purchaseCostMultiplier, currentlyOwned));
    }
}

// -------------------------------------------------------------------------

[System.Serializable]
public class StatUpgradeTrack
{
    public string upgradeName = "Upgrade";
    public int maxLevels = 10;
    public int costPerLevel = 0;
    [Tooltip("How much the stat increases per level")]
    public float incrementPerLevel = 5f;
}

[System.Serializable]
public class AbilityUpgradeLevelData
{
    [Tooltip("Flat boost added to ability power at this level. Set to 0 for a cooldown level.")]
    public float abilityBoostIncrement = 0f;
    [Tooltip("Seconds removed from cooldown at this level. Set to 0 for a boost level.")]
    public float cooldownReduction = 0f;
    [Tooltip("Cost for this specific level — lets you make later levels more expensive")]
    public int cost = 0;
}

[System.Serializable]
public class AbilityUpgradeTrack
{
    public string upgradeName = "Ability";
    [Tooltip("Level 1 = unlock. Levels 2-10 = improvements. " +
            "Set abilityBoostIncrement OR cooldownReduction per level to control what alternates.")]
    public AbilityUpgradeLevelData[] levels = new AbilityUpgradeLevelData[10];
}