using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DuckController duckController;
    [SerializeField] private PlayerInventory inventory;

    private DuckDefinition Def => duckController?.Definition;
    private DuckFlightController Flight =>
        duckController?.GetComponent<DuckFlightController>();
    private AbilityController Ability =>
        duckController?.GetComponent<AbilityController>();

    public DuckDefinition Definition => duckController?.Definition;
    public int SpeedLevel   => inventory != null && Def != null
        ? inventory.GetSpeedLevel(Def)   : 0;
    public int ManeurLevel  => inventory != null && Def != null
        ? inventory.GetManeurLevel(Def)  : 0;
    public int AbilityLevel => inventory != null && Def != null
        ? inventory.GetAbilityLevel(Def) : 0;

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

    private void Start()
    {
        Invoke(nameof(ApplyCurrentStats), 0.1f);
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

        Flight?.SetMaxSpeed(CalcSpeed(newLevel));
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

        Flight?.SetManoeuvrability(CalcTurnSpeed(newLevel));
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

        if (newLevel == 1)
            Ability?.UnlockAbility();
        else
            Ability?.ApplyAbilityUpgrade(
                levelData.abilityBoostIncrement,
                levelData.cooldownReduction);

        UpgradeUI.Instance?.Refresh();
        return true;
    }

    public void ApplyCurrentStats()
    {
        if (Def == null || Flight == null)
        {
            Debug.LogWarning("ApplyCurrentStats: Def or Flight null");
            return;
        }

        Flight.SetMaxSpeed(CalcSpeed(SpeedLevel));
        Flight.SetManoeuvrability(CalcTurnSpeed(ManeurLevel));

        if (Ability != null)
        {
            if (AbilityLevel >= 1)
                Ability.UnlockAbility();

            float totalBoost = 0f;
            float totalCooldown = 0f;
            for (int i = 1; i < AbilityLevel &&
                i < Def.abilityUpgrade.levels.Length; i++)
            {
                totalBoost    += Def.abilityUpgrade.levels[i].abilityBoostIncrement;
                totalCooldown += Def.abilityUpgrade.levels[i].cooldownReduction;
            }
            if (AbilityLevel > 1)
                Ability.ApplyAbilityUpgrade(totalBoost, totalCooldown);
        }

        Debug.Log($"ApplyCurrentStats complete — maxSpeed: {CalcSpeed(SpeedLevel)}, " +
                  $"turnSpeed: {CalcTurnSpeed(ManeurLevel)}, abilityLevel: {AbilityLevel}");
    }

    // -------------------------------------------------------------------------

    // Sum increments for all purchased levels
    private float CalcSpeed(int level)
    {
        float val = Def.baseMaxSpeed;
        for (int i = 0; i < level && i < Def.maxSpeedUpgrade.levels.Length; i++)
            val += Def.maxSpeedUpgrade.levels[i].statIncrement;
        return val;
    }

    private float CalcTurnSpeed(int level)
    {
        float val = Def.baseTurnSpeed;
        for (int i = 0; i < level && i < Def.manoeuvrabilityUpgrade.levels.Length; i++)
            val += Def.manoeuvrabilityUpgrade.levels[i].statIncrement;
        return val;
    }

    private bool CanAfford(int cost)
    {
        if (cost <= 0) return true;
        return CurrencyManager.Instance != null &&
            CurrencyManager.Instance.CanAfford(cost);
    }
}