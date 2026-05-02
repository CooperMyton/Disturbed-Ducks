using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI duckNameText;

    [Header("Level Labels")]
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI maneurLevelText;
    [SerializeField] private TextMeshProUGUI abilityLevelText;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button speedButton;
    [SerializeField] private Button maneurButton;
    [SerializeField] private Button abilityButton;
    [SerializeField] private TextMeshProUGUI abilityButtonText;
    [SerializeField] private TextMeshProUGUI speedButtonText;
    [SerializeField] private TextMeshProUGUI maneurButtonText;

    [Header("Buy Duck")]
    [SerializeField] private Button buyDuckButton;
    [SerializeField] private TextMeshProUGUI buyDuckButtonText;

    // -------------------------------------------------------------------------

    public static UpgradeUI Instance { get; private set; }

    private void Awake()
    {
        Debug.Log($"UpgradeUI Awake on: {gameObject.name} — duplicate: {Instance != null}");
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); // ← was Destroy(gameObject), only remove the component
            return; 
        }
        Instance = this;

        speedButton?.onClick.AddListener(() => OnUpgradeClicked(0));
        maneurButton?.onClick.AddListener(() => OnUpgradeClicked(1));
        abilityButton?.onClick.AddListener(() => OnUpgradeClicked(2));
        buyDuckButton?.onClick.AddListener(OnBuyDuckClicked);
        Hide();
    }

    private void Start()
    {
        Invoke(nameof(InitializeUI), 0.15f);
    }

    private void InitializeUI()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeManager.Instance is null");
            return;
        }
        if (UpgradeManager.Instance.Definition == null)
        {
            Debug.LogError("DuckDefinition is null — assign BasicDuck on DuckController");
            return;
        }
        Refresh();
    }

    // -------------------------------------------------------------------------

    public void Show()  { upgradePanel.SetActive(true);  Refresh(); }
    public void Hide()  { upgradePanel.SetActive(false); }

    public void Refresh()
    {
        if (UpgradeManager.Instance == null) return;
        var um  = UpgradeManager.Instance;
        var def = um.Definition;
        if (def == null) return;

        if (duckNameText != null)
            duckNameText.text = def.duckName;

        // Speed
        if (speedLevelText != null)
            speedLevelText.text =
                $"{def.maxSpeedUpgrade.upgradeName}: " +
                $"{um.SpeedLevel} / {def.maxSpeedUpgrade.levels.Length}";
        if (speedButton != null)
            speedButton.interactable = um.CanUpgradeSpeed;
        if (speedButtonText != null)
            speedButtonText.text = GetStatButtonLabel(
                um.CanUpgradeSpeed,
                um.SpeedLevel,
                def.maxSpeedUpgrade.levels);

        // Manoeuvrability
        if (maneurLevelText != null)
            maneurLevelText.text =
                $"{def.manoeuvrabilityUpgrade.upgradeName}: " +
                $"{um.ManeurLevel} / {def.manoeuvrabilityUpgrade.levels.Length}";
        if (maneurButton != null)
            maneurButton.interactable = um.CanUpgradeManeur;
        if (maneurButtonText != null)
            maneurButtonText.text = GetStatButtonLabel(
                um.CanUpgradeManeur,
                um.ManeurLevel,
                def.manoeuvrabilityUpgrade.levels);

        // Ability
        bool abilityLocked = um.AbilityLevel == 0;
        if (abilityLevelText != null)
            abilityLevelText.text = abilityLocked
                ? $"{def.abilityUpgrade.upgradeName}: LOCKED"
                : $"{def.abilityUpgrade.upgradeName}: " +
                $"{um.AbilityLevel} / {def.abilityUpgrade.levels.Length}";
        if (abilityButton != null)
            abilityButton.interactable = um.CanUpgradeAbility;
        if (abilityButtonText != null)
            abilityButtonText.text = GetAbilityButtonLabel(um, def, abilityLocked);

        // Buy duck
        RefreshBuyDuck(def);
    }

    // -------------------------------------------------------------------------

    private string GetStatButtonLabel(bool canUpgrade, int currentLevel,
        StatUpgradeLevelData[] levels)
    {
        if (!canUpgrade) return "MAX";
        int cost = levels[currentLevel].cost;
        return cost > 0 ? $"UPGRADE ({cost} coins)" : "UPGRADE Free";
    }

    private string GetAbilityButtonLabel(UpgradeManager um, DuckDefinition def,
        bool locked)
    {
        if (!um.CanUpgradeAbility) return "MAX";
        int cost = def.abilityUpgrade.levels[um.AbilityLevel].cost;
        string action = locked ? "UNLOCK" : "UPGRADE";
        return cost > 0 ? $"{action} ({cost} coins)" : $"{action} (Free)";
    }

    private void RefreshBuyDuck(DuckDefinition def)
    {
        Debug.Log($"RefreshBuyDuck — buyDuckButton null: {buyDuckButton == null}, " +
                $"buyDuckButtonText null: {buyDuckButtonText == null}, " +
                $"inv null: {PlayerDuckInventory.Instance == null}, " +
                $"currency null: {CurrencyManager.Instance == null}");

        if (buyDuckButton == null || buyDuckButtonText == null) return;

        var inv = PlayerDuckInventory.Instance;
        if (inv == null) return;

        bool full = inv.TotalOwned >= inv.MaxTotalDucks;
        int cost = def.GetPurchaseCost(inv.GetOwned(def));
        bool canAfford = CurrencyManager.Instance != null &&
                        CurrencyManager.Instance.CanAfford(cost);

        Debug.Log($"RefreshBuyDuck — full: {full}, cost: {cost}, " +
                $"canAfford: {canAfford}, balance: {CurrencyManager.Instance?.Balance}");

        buyDuckButton.interactable = !full && canAfford;
        buyDuckButtonText.text = full
            ? "LOADOUT FULL"
            : cost > 0 ? $"BUY DUCK ({cost} coins)" : "BUY DUCK (Free)";
    }

    private void OnUpgradeClicked(int track)
    {
        switch (track)
        {
            case 0: UpgradeManager.Instance?.TryUpgradeSpeed();           break;
            case 1: UpgradeManager.Instance?.TryUpgradeManoeuvrability(); break;
            case 2: UpgradeManager.Instance?.TryUpgradeAbility();         break;
        }
        Refresh();
    }

    private void OnBuyDuckClicked()
    {
        var def = UpgradeManager.Instance?.Definition;
        Debug.Log($"OnBuyDuckClicked — def null: {def == null}, " +
                $"balance: {CurrencyManager.Instance?.Balance}, " +
                $"cost: {def?.GetPurchaseCost(PlayerDuckInventory.Instance?.GetOwned(def) ?? 0)}");
        if (def == null) return;
        PlayerDuckInventory.Instance?.TryBuyDuck(def);
        Refresh();
    }
    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged += OnBalanceChanged;
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged -= OnBalanceChanged;
    }

    private void OnBalanceChanged(int _) => Refresh();
}