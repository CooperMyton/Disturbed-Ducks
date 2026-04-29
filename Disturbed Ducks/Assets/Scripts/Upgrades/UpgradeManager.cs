using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private DuckController duckController;

    private int _speedLevel = 0;
    private int _maneurLevel = 0;
    private int _abilityLevel = 0; // 0 = locked

    // Convenience reference
    private DuckDefinition Def => duckController.Definition;
    private DuckFlightController Flight =>
        duckController.GetComponent<DuckFlightController>();
    private AbilityController Ability =>
        duckController.GetComponent<AbilityController>();

    // Read by UpgradeUI
    public int SpeedLevel     => _speedLevel;
    public int ManeurLevel    => _maneurLevel;
    public int AbilityLevel   => _abilityLevel;

    public bool CanUpgradeSpeed   => _speedLevel   < Def.maxSpeedUpgrade.maxLevels;
    public bool CanUpgradeManeur  => _maneurLevel  < Def.manoeuvrabilityUpgrade.maxLevels;
    public bool CanUpgradeAbility => _abilityLevel < Def.abilityUpgrade.levels.Length;

        public DuckDefinition Definition => duckController?.Definition;
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // -------------------------------------------------------------------------

    public bool TryUpgradeSpeed()
    {
        if (!CanUpgradeSpeed) return false;
        if (!CanAfford(Def.maxSpeedUpgrade.costPerLevel)) return false;

        _speedLevel++;
        float newMax = Def.baseMaxSpeed +
            (_speedLevel * Def.maxSpeedUpgrade.incrementPerLevel);
        Flight?.SetMaxSpeed(newMax);

        UpgradeUI.Instance?.Refresh();
        return true;
    }

    public bool TryUpgradeManoeuvrability()
    {
        if (!CanUpgradeManeur) return false;
        if (!CanAfford(Def.manoeuvrabilityUpgrade.costPerLevel)) return false;

        _maneurLevel++;
        float newTurn = Def.baseTurnSpeed +
            (_maneurLevel * Def.manoeuvrabilityUpgrade.incrementPerLevel);
        Flight?.SetManoeuvrability(newTurn);
        UpgradeUI.Instance?.Refresh();
        return true;
    }

    public bool TryUpgradeAbility()
    {
        if (!CanUpgradeAbility) return false;

        var levelData = Def.abilityUpgrade.levels[_abilityLevel];
        if (!CanAfford(levelData.cost)) return false;

        _abilityLevel++;

        if (_abilityLevel == 1)
        {
            // First level = unlock only, no stat boost yet
            Ability?.UnlockAbility();
        }
        else
        {
            Ability?.ApplyAbilityUpgrade(
                levelData.abilityBoostIncrement,
                levelData.cooldownReduction
            );
        }
        UpgradeUI.Instance?.Refresh();
        return true;
    }

    // -------------------------------------------------------------------------

    private bool CanAfford(int cost)
    {
        // Free for POC — wire CurrencyManager here later
        // if (cost > 0 && CurrencyManager.Instance.Balance < cost) return false;
        return true;
    }
}