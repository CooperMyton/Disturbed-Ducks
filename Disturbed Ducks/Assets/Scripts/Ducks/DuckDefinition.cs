using UnityEngine;

[CreateAssetMenu(fileName = "NewDuck", menuName = "Ducks/Duck Definition")]
public class DuckDefinition : ScriptableObject
{
    [Header("Identity")]
    public string duckName = "Basic Duck";

    [Header("Purchase")]
    public int[] purchaseCosts = new int[] { 50 };
    public int maxOwned = 3;

    [Header("Base Stats")]
    public float baseMaxSpeed     = 35f;
    public float baseTurnSpeed    = 70f;
    public float baseGlideGravity = 12f;
    public float baseMinSpeed     = 5f;
    public float baseMass         = 1f;

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
        upgradeName = "Manoeuvrability & Mass"
    };

    public int GetPurchaseCost(int currentlyOwned)
    {
        if (purchaseCosts == null || purchaseCosts.Length == 0) return 0;
        int index = Mathf.Clamp(currentlyOwned, 0, purchaseCosts.Length - 1);
        return purchaseCosts[index];
    }

    [Header("Flight")]
    public bool disableFlightControls = false;

    [Header("Stats Display")]
    public DuckStatDisplay statDisplay = new DuckStatDisplay();
}

// -------------------------------------------------------------------------

[System.Serializable]
public class StatUpgradeLevelData
{
    public float statIncrement = 5f;
    public float massIncrement = 0f; // added — leave 0 for ducks that don't upgrade mass
    public int   cost          = 0;
}

[System.Serializable]
public class StatUpgradeTrack
{
    public string               upgradeName = "Upgrade";
    public StatUpgradeLevelData[] levels    = new StatUpgradeLevelData[10];
}
// Add this class at the bottom of DuckDefinition.cs alongside StatUpgradeTrack
[System.Serializable]
public class DuckStatDisplay
{
    public bool showMaxSpeed     = true;
    public bool showTurnSpeed    = true;
    public bool showMass         = true;
    public bool showGravity      = false;
    public bool showMinSpeed     = false;
    public bool showAbilityStats = true;
}