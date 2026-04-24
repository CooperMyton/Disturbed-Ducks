using UnityEngine;

/// <summary>
/// Tracks current upgrade levels and applies stats to the duck.
/// Singleton so UpgradeUI can reach it without a direct reference.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DuckData duckData;
    [SerializeField] private DuckFlightController flightController;

    private int _speedLevel = 0;

    // Read-only properties for UI
    public int SpeedLevel       => _speedLevel;
    public int MaxSpeedLevel    => duckData.maxSpeedLevel;
    public string DuckName      => duckData.duckName;
    public bool CanUpgradeSpeed => _speedLevel < duckData.maxSpeedLevel;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ApplyStats();
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts a speed upgrade. Returns true if successful.
    /// Currency check is stubbed out — wire up CurrencyManager here later.
    /// </summary>
    public bool TryUpgradeSpeed()
    {
        if (!CanUpgradeSpeed) return false;

        // ----- Currency check (inactive for POC) -----
        // if (CurrencyManager.Instance.Balance < duckData.speedUpgradeCostPerLevel)
        //     return false;
        // CurrencyManager.Instance.Spend(duckData.speedUpgradeCostPerLevel);
        // ---------------------------------------------

        _speedLevel++;
        ApplyStats();
        return true;
    }

    // -------------------------------------------------------------------------

    private void ApplyStats()
    {
        if (flightController == null) return;
        float newSpeed = duckData.baseSpeed + (_speedLevel * duckData.speedUpgradeIncrement);
        flightController.SetBaseSpeed(newSpeed);
    }
}