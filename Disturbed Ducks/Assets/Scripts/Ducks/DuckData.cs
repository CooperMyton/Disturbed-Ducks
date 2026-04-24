using UnityEngine;

[CreateAssetMenu(fileName = "NewDuckData", menuName = "Ducks/Duck Data")]
public class DuckData : ScriptableObject
{
    [Header("Identity")]
    public string duckName = "Basic Duck";

    [Header("Max Speed")]
    [Tooltip("The speed cap — launcher determines launch speed, this caps how fast the duck can ever go")]
    public float baseMaxSpeed = 35f;
    public float maxSpeedUpgradeIncrement = 5f;   // added per level
    public int maxSpeedLevel = 10;

    [Header("Currency — inactive for POC, ready for later")]
    [Tooltip("Set to 0 for free upgrades. Wire up CurrencyManager here later.")]
    public int upgradeCostPerLevel = 0;
}