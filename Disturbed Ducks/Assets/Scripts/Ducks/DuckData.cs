using UnityEngine;

/// <summary>
/// ScriptableObject that defines a duck's base stats and upgrade caps.
/// Create one asset per duck type via Assets > Create > Ducks > Duck Data.
/// To add a new duck later: duplicate this asset and change the values.
/// </summary>
[CreateAssetMenu(fileName = "NewDuckData", menuName = "Ducks/Duck Data")]
public class DuckData : ScriptableObject
{
    [Header("Identity")]
    public string duckName = "Basic Duck";

    [Header("Speed")]
    public float baseSpeed = 20f;
    public float speedUpgradeIncrement = 3f;   // added per level
    public int maxSpeedLevel = 10;

    [Header("Currency — inactive for POC, ready for later")]
    [Tooltip("Set to 0 for free upgrades. Wire up CurrencyManager here later.")]
    public int speedUpgradeCostPerLevel = 0;
}