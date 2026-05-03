using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DuckController duckController;
    [SerializeField] private PlayerInventory inventory;

    private DuckDefinition _overrideDuck;

    private DuckDefinition Def       => _overrideDuck ?? duckController?.Definition;
    private DuckDefinition FlyingDef => duckController?.Definition;
    private bool TabMatchesFlyingDuck => _overrideDuck == null || _overrideDuck == FlyingDef;

    private DuckFlightController Flight =>
        duckController?.GetComponent<DuckFlightController>();
    private AbilityController Ability =>
        duckController?.GetComponent<AbilityController>();

    public DuckDefinition Definition       => Def;
    public DuckDefinition FlyingDefinition => FlyingDef;

    public int SpeedLevel   => inventory != null && Def != null ? inventory.GetSpeedLevel(Def)   : 0;
    public int ManeurLevel  => inventory != null && Def != null ? inventory.GetManeurLevel(Def)  : 0;
    public int AbilityLevel => inventory != null && Def != null ? inventory.GetAbilityLevel(Def) : 0;

    public bool CanUpgradeSpeed =>
        Def != null && SpeedLevel < Def.maxSpeedUpgrade.levels.Length;
    public bool CanUpgradeManeur =>
        Def != null && ManeurLevel < Def.manoeuvrabilityUpgrade.levels.Length;
    public bool CanUpgradeAbility =>
        Def != null && AbilityLevel < Def.abilityUpgrade.levels.Length;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => Invoke(nameof(ApplyCurrentStats), 0.1f);

    public void SetActiveDuck(DuckDefinition def)
    {
        _overrideDuck = def;
        UpgradeUI.Instance?.Refresh();
    }

    public void ClearActiveDuckOverride()
    {
        _overrideDuck = null;
        UpgradeUI.Instance?.Refresh();
    }

    // -------------------------------------------------------------------------

    public bool TryUpgradeSpeed()
    {
        if (!CanUpgradeSpeed) return false;
        var levelData = Def.maxSpeedUpgrade.levels[SpeedLevel];
        if (!CanAfford(levelData.cost)) return false;
        if (levelData.cost > 0) CurrencyManager.Instance?.Spend(levelData.cost);

        int newLevel = SpeedLevel + 1;
        inventory.SetSpeedLevel(Def, newLevel);

        if (TabMatchesFlyingDuck)
            Flight?.SetMaxSpeed(CalcSpeed(Def, newLevel));

        UpgradeUI.Instance?.Refresh();
        return true;
    }

    public bool TryUpgradeManoeuvrability()
    {
        if (!CanUpgradeManeur) return false;
        var levelData = Def.manoeuvrabilityUpgrade.levels[ManeurLevel];
        if (!CanAfford(levelData.cost)) return false;
        if (levelData.cost > 0) CurrencyManager.Instance?.Spend(levelData.cost);

        int newLevel = ManeurLevel + 1;
        inventory.SetManeurLevel(Def, newLevel);

        if (TabMatchesFlyingDuck)
            Flight?.SetManoeuvrability(CalcTurnSpeed(Def, newLevel));

        UpgradeUI.Instance?.Refresh();
        return true;
    }

    public bool TryUpgradeAbility()
    {
        if (!CanUpgradeAbility) return false;
        var levelData = Def.abilityUpgrade.levels[AbilityLevel];
        if (!CanAfford(levelData.cost)) return false;
        if (levelData.cost > 0) CurrencyManager.Instance?.Spend(levelData.cost);

        int newLevel = AbilityLevel + 1;
        inventory.SetAbilityLevel(Def, newLevel);

        if (TabMatchesFlyingDuck)
        {
            if (newLevel == 1)
                Ability?.UnlockAbility();
            else
                Ability?.ApplyAbilityUpgrade(
                    levelData.abilityBoostIncrement,
                    levelData.cooldownReduction,
                    levelData.radiusIncrement,
                    levelData.damageIncrement,
                    levelData.explosionDelayReduction);
        }

        UpgradeUI.Instance?.Refresh();
        return true;
    }

    public void ApplyCurrentStats()
    {
        var def = FlyingDef;
        if (def == null || Flight == null)
        {
            Debug.LogWarning("ApplyCurrentStats: FlyingDef or Flight null");
            return;
        }

        int speedLvl   = inventory.GetSpeedLevel(def);
        int maneurLvl  = inventory.GetManeurLevel(def);
        int abilityLvl = inventory.GetAbilityLevel(def);

        Flight.SetMaxSpeed(CalcSpeed(def, speedLvl));
        Flight.SetManoeuvrability(CalcTurnSpeed(def, maneurLvl));

        if (Ability != null)
        {
            if (abilityLvl >= 1) Ability.UnlockAbility();
            else                 Ability.LockAbility();

            float tBoost = 0, tCooldown = 0, tRadius = 0, tDamage = 0, tDelay = 0;
            for (int i = 1; i < abilityLvl && i < def.abilityUpgrade.levels.Length; i++)
            {
                var lvl   = def.abilityUpgrade.levels[i];
                tBoost   += lvl.abilityBoostIncrement;
                tCooldown += lvl.cooldownReduction;
                tRadius  += lvl.radiusIncrement;
                tDamage  += lvl.damageIncrement;
                tDelay   += lvl.explosionDelayReduction;
            }
            Ability.SetAbilityUpgrades(tBoost, tCooldown, tRadius, tDamage, tDelay);
        }

        Debug.Log($"ApplyCurrentStats — maxSpeed: {CalcSpeed(def, speedLvl)}, " +
                  $"turnSpeed: {CalcTurnSpeed(def, maneurLvl)}, abilityLevel: {abilityLvl}");
    }

    // -------------------------------------------------------------------------

    private float CalcSpeed(DuckDefinition def, int level)
    {
        float val = def.baseMaxSpeed;
        for (int i = 0; i < level && i < def.maxSpeedUpgrade.levels.Length; i++)
            val += def.maxSpeedUpgrade.levels[i].statIncrement;
        return val;
    }

    private float CalcTurnSpeed(DuckDefinition def, int level)
    {
        float val = def.baseTurnSpeed;
        for (int i = 0; i < level && i < def.manoeuvrabilityUpgrade.levels.Length; i++)
            val += def.manoeuvrabilityUpgrade.levels[i].statIncrement;
        return val;
    }

    private bool CanAfford(int cost)
    {
        if (cost <= 0) return true;
        return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(cost);
    }
}