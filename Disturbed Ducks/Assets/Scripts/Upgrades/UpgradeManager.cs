using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DuckData duckData;
    [SerializeField] private DuckFlightController flightController;

    private int _maxSpeedLevel = 0;

    public int SpeedLevel        => _maxSpeedLevel;
    public int MaxSpeedLevel     => duckData.maxSpeedLevel;
    public string DuckName       => duckData.duckName;
    public bool CanUpgradeSpeed  => _maxSpeedLevel < duckData.maxSpeedLevel;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => ApplyStats();

    public bool TryUpgradeSpeed()
    {
        if (!CanUpgradeSpeed) return false;

        // ----- Currency check (inactive for POC) -----
        // if (CurrencyManager.Instance.Balance < duckData.upgradeCostPerLevel) return false;
        // CurrencyManager.Instance.Spend(duckData.upgradeCostPerLevel);
        // ---------------------------------------------

        _maxSpeedLevel++;
        ApplyStats();
        return true;
    }

    private void ApplyStats()
    {
        if (flightController == null) return;
        float newMaxSpeed = duckData.baseMaxSpeed + (_maxSpeedLevel * duckData.maxSpeedUpgradeIncrement);
        flightController.SetMaxSpeed(newMaxSpeed);
    }
}