using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance { get; private set; }

    [Header("Outer Panel")]
    [SerializeField] private GameObject      upgradePanel;
    [SerializeField] private TextMeshProUGUI duckNameText;
    [SerializeField] private TextMeshProUGUI duckDescriptionText;
    [SerializeField] private TextMeshProUGUI abilityDescriptionText;

    [Header("Tab System")]
    [SerializeField] private GameDefinition gameDefinition;
    [SerializeField] private Transform      tabContainer;
    [SerializeField] private Color tabSelectedColor   = Color.white;
    [SerializeField] private Color tabUnselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color tabUnownedColor    = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.18f, 0.18f, 0.18f, 1f);


    [Header("Upgrade Content — shown when duck is owned")]
    [SerializeField] private GameObject      upgradeContent;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI maneurLevelText;
    [SerializeField] private TextMeshProUGUI abilityLevelText;
    [SerializeField] private Button          speedButton;
    [SerializeField] private Button          maneurButton;
    [SerializeField] private Button          abilityButton;
    [SerializeField] private TextMeshProUGUI speedButtonText;
    [SerializeField] private TextMeshProUGUI maneurButtonText;
    [SerializeField] private TextMeshProUGUI abilityButtonText;
    [SerializeField] private Button          buyDuckButton;
    [SerializeField] private TextMeshProUGUI buyDuckButtonText;

    [Header("Purchase Content — shown when duck is not yet owned")]
    [SerializeField] private GameObject      purchaseContent;
    [SerializeField] private TextMeshProUGUI purchaseTitleText;
    [SerializeField] private TextMeshProUGUI purchaseCostText;
    [SerializeField] private Button          purchaseUnlockButton;
    [SerializeField] private TextMeshProUGUI purchaseUnlockButtonText;

    [Header("Sell Duck Button")]
    [SerializeField] private Button          sellDuckButton;
    [SerializeField] private TextMeshProUGUI sellDuckButtonText;
    private DuckDefinition _selectedTab;

    private readonly List<(Button btn, Image img)> _tabData
        = new List<(Button, Image)>();

    [Header("Stats Panel")]
    [SerializeField] private Transform statsContainer;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        speedButton?.onClick.AddListener(()  => OnUpgradeClicked(0));
        maneurButton?.onClick.AddListener(() => OnUpgradeClicked(1));
        abilityButton?.onClick.AddListener(()=> OnUpgradeClicked(2));
        buyDuckButton?.onClick.AddListener(OnBuyDuckClicked);
        purchaseUnlockButton?.onClick.AddListener(OnPurchaseUnlockClicked);
        sellDuckButton?.onClick.AddListener(OnSellDuckClicked);

        Hide();
    }

    private void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged += OnBalanceChanged;

        Invoke(nameof(InitializeUI), 0.15f);
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged -= OnBalanceChanged;
    }

    private void OnBalanceChanged(int _) => Refresh();

    private void InitializeUI()
    {
        if (UpgradeManager.Instance == null) { Debug.LogError("UpgradeUI: UpgradeManager null"); return; }
        BuildTabs();
        SelectTab(UpgradeManager.Instance.FlyingDefinition
            ?? gameDefinition?.allDucks?[0]);
    }

    // -------------------------------------------------------------------------

    public void Show()
    {
        upgradePanel.SetActive(true);
        var flyingDef = UpgradeManager.Instance?.FlyingDefinition;
        if (flyingDef != null) SelectTab(flyingDef);
        else Refresh();
    }

    public void Hide()
    {
        upgradePanel?.SetActive(false);
        UpgradeManager.Instance?.ClearActiveDuckOverride();
    }

    public void Refresh()
    {
        if (_selectedTab == null) return;

        RefreshTabs();

        bool owned = (PlayerDuckInventory.Instance?.GetOwned(_selectedTab) ?? 0) > 0;
        bool unlocked = PlayerDuckInventory.Instance?.IsDuckUnlocked(_selectedTab) ?? false;

        if (upgradeContent  != null) upgradeContent.SetActive(owned);
        if (purchaseContent != null) purchaseContent.SetActive(!owned);
        if (duckNameText    != null) duckNameText.text = _selectedTab.duckName;
        if (duckDescriptionText != null)
            duckDescriptionText.text = _selectedTab.duckDescription;

        if (abilityDescriptionText != null)
            abilityDescriptionText.text = _selectedTab.ability != null
                ? _selectedTab.ability.abilityDescription
                : "No active ability.";
        if (statsContainer  != null) statsContainer.gameObject.SetActive(owned);

        if (owned) RefreshUpgradeContent();
        else       RefreshPurchaseContent(unlocked);

    }

    // -------------------------------------------------------------------------

    private void BuildTabs()
    {
        if (tabContainer == null || gameDefinition == null) return;

        foreach (Transform child in tabContainer) Destroy(child.gameObject);
        _tabData.Clear();

        foreach (var duck in gameDefinition.allDucks)
        {
            var go = new GameObject(duck.duckName + "_Tab");
            go.transform.SetParent(tabContainer, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;

            var img = go.AddComponent<Image>();
            img.color = tabUnselectedColor;

            var btn = go.AddComponent<Button>();

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = textRt.offsetMax = Vector2.zero;

            var tmp           = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text          = duck.duckName;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.fontSize      = 13;
            tmp.raycastTarget = false;

            var capturedDuck = duck;
            btn.onClick.AddListener(() => SelectTab(capturedDuck));

            go.SetActive(true);
            _tabData.Add((btn, img));
        }
    }

    private void SelectTab(DuckDefinition def)
    {
        if (def == null) return;
        _selectedTab = def;
        UpgradeManager.Instance?.SetActiveDuck(def);
        Refresh();
    }

    private void RefreshTabs()
    {
        if (gameDefinition == null) return;
        for (int i = 0; i < gameDefinition.allDucks.Count && i < _tabData.Count; i++)
        {
            var duck        = gameDefinition.allDucks[i];
            var (_, img)    = _tabData[i];
            bool isSelected = duck == _selectedTab;
            bool owned = (PlayerDuckInventory.Instance?.GetOwned(duck) ?? 0) > 0;
            bool unlocked = PlayerDuckInventory.Instance?.IsDuckUnlocked(duck) ?? false;

            img.color = isSelected ? tabSelectedColor
                        : owned      ? tabUnselectedColor
                        : unlocked   ? tabUnownedColor
                                    : lockedColor;

        }
    }

    private void RefreshUpgradeContent()
    {
        var um  = UpgradeManager.Instance;
        if (um == null) return;
        var def = _selectedTab;

        // Speed
        if (speedLevelText != null)
            speedLevelText.text =
                $"{def.maxSpeedUpgrade.upgradeName}: {um.SpeedLevel} / {def.maxSpeedUpgrade.levels.Length}";
        if (speedButton != null)     speedButton.interactable = um.CanUpgradeSpeed;
        if (speedButtonText != null) speedButtonText.text =
            GetStatLabel(um.CanUpgradeSpeed, um.SpeedLevel,
                        def.maxSpeedUpgrade.levels, "Speed");

        // Manoeuvrability
        if (maneurLevelText != null)
            maneurLevelText.text =
                $"{def.manoeuvrabilityUpgrade.upgradeName}: {um.ManeurLevel} / {def.manoeuvrabilityUpgrade.levels.Length}";
        if (maneurButton != null)     maneurButton.interactable = um.CanUpgradeManeur;
        if (maneurButtonText != null) maneurButtonText.text =
            GetStatLabel(um.CanUpgradeManeur, um.ManeurLevel,
                        def.manoeuvrabilityUpgrade.levels, "Turn", hasMass: true);
        // Ability — data now comes from the ability asset itself
        bool hasAbility = def.ability != null;
        bool locked     = um.AbilityLevel == 0;

        if (abilityLevelText != null)
            abilityLevelText.text = !hasAbility ? "No Ability"
                : locked ? $"{def.ability.abilityName}: LOCKED"
                        : $"{def.ability.abilityName}: {um.AbilityLevel} / {def.ability.MaxUpgradeLevels}";

        if (abilityButton != null)     abilityButton.interactable = um.CanUpgradeAbility;
        if (abilityButtonText != null) abilityButtonText.text     = GetAbilityLabel(um, def, locked);

        RefreshBuyDuck(def);
        RefreshSellDuck(def);
        BuildStatsPanel(def);
    }

    private void RefreshPurchaseContent(bool unlocked)
    {
        if (_selectedTab == null) return;

        int cost = _selectedTab.GetPurchaseCost(0);
        bool canAfford = CurrencyManager.Instance?.CanAfford(cost) ?? false;

        if (!unlocked)
        {
            if (purchaseTitleText != null)
                purchaseTitleText.text = $"Locked: {_selectedTab.duckName}";
            if (purchaseCostText != null)
                purchaseCostText.text = _selectedTab.unlockRequirementText;
            if (purchaseUnlockButton != null)
                purchaseUnlockButton.interactable = false;
            if (purchaseUnlockButtonText != null)
                purchaseUnlockButtonText.text = "LOCKED";
            return;
        }

        if (purchaseTitleText != null)
            purchaseTitleText.text = $"Unlock {_selectedTab.duckName}";
        if (purchaseCostText != null)
            purchaseCostText.text = cost > 0 ? $"Cost: {cost} coins" : "Cost: Free";
        if (purchaseUnlockButton != null)
            purchaseUnlockButton.interactable = canAfford;
        if (purchaseUnlockButtonText != null)
            purchaseUnlockButtonText.text = cost > 0 ? $"PURCHASE ({cost} coins)" : "PURCHASE (Free)";
    }


    private void RefreshBuyDuck(DuckDefinition def)
    {
        if (buyDuckButton == null || buyDuckButtonText == null) return;
        var inv = PlayerDuckInventory.Instance;
        if (inv == null) return;

        int  owned     = inv.GetOwned(def);
        bool totalFull = inv.TotalOwned >= inv.MaxTotalDucks;
        bool typeFull  = owned >= def.maxOwned;           // per-duck cap
        bool full      = totalFull || typeFull;
        int  cost      = def.GetPurchaseCost(owned);
        bool canAfford = CurrencyManager.Instance?.CanAfford(cost) ?? false;

        buyDuckButton.interactable = !full && canAfford;
        buyDuckButtonText.text = typeFull  ? "MAX OWNED"
            : totalFull ? "LOADOUT FULL"
            : cost > 0  ? $"BUY DUCK ({cost} coins)"
                        : "BUY DUCK (Free)";
    }

    // -------------------------------------------------------------------------

    private string GetStatLabel(bool canUpgrade, int currentLevel,
                                StatUpgradeLevelData[] levels,
                                string statName, bool hasMass = false)
    {
        if (currentLevel >= levels.Length) return "MAX";
        var level = levels[currentLevel];
        var parts = new List<string>();
        if (level.statIncrement > 0 && !string.IsNullOrEmpty(statName))
            parts.Add($"+{level.statIncrement:F0} {statName}");
        if (hasMass && level.massIncrement > 0)
            parts.Add($"+{level.massIncrement:F2} Mass");
        string preview = parts.Count > 0 ? $" ({string.Join(", ", parts)})" : "";
        string costStr  = level.cost > 0 ? $" — {level.cost} coins" : "";
        return $"UPGRADE{preview}{costStr}";
    }

    private string GetAbilityLabel(UpgradeManager um, DuckDefinition def, bool locked)
    {
        if (def.ability == null) return "N/A";
        if (um.AbilityLevel >= def.ability.MaxUpgradeLevels) return "MAX";
        string preview  = def.ability.GetUpgradePreview(um.AbilityLevel);
        string previewStr = !string.IsNullOrEmpty(preview) ? $" ({preview})" : "";
        int    cost     = def.ability.GetUpgradeCost(um.AbilityLevel);
        string action   = locked ? "UNLOCK" : "UPGRADE";
        string costStr  = cost > 0 ? $" — {cost} coins" : "";
        return $"{action} ABILITY (Left Shift){previewStr}{costStr}";
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
        if (_selectedTab == null) return;
        PlayerDuckInventory.Instance?.TryBuyDuck(_selectedTab);
        Refresh();
    }

    private void OnPurchaseUnlockClicked()
    {
        if (_selectedTab == null) return;
        if (PlayerDuckInventory.Instance?.TryBuyDuck(_selectedTab) == true)
            Refresh();
    }

    private void RefreshSellDuck(DuckDefinition def)
    {
        if (sellDuckButton == null || sellDuckButtonText == null) return;
        var inv = PlayerDuckInventory.Instance;
        if (inv == null) return;

        bool canSell  = inv.CanSellDuck(def);
        int  sellPrice = inv.GetSellPrice(def);

        sellDuckButton.interactable = canSell;
        sellDuckButtonText.text     = canSell
            ? $"SELL DUCK (+{sellPrice} coins)"
            : "CANNOT SELL";
    }

    private void OnSellDuckClicked()
    {
        if (_selectedTab == null) return;
        PlayerDuckInventory.Instance?.TrySellDuck(_selectedTab);
        Refresh();
    }
    private void BuildStatsPanel(DuckDefinition def)
    {
        if (statsContainer == null) return;
        foreach (Transform child in statsContainer) Destroy(child.gameObject);

        var um      = UpgradeManager.Instance;
        if (um == null) return;
        var display = def.statDisplay;

        if (display.showMaxSpeed)  AddStatRow($"Max Speed:   {um.CurrentMaxSpeed:F1}");
        if (display.showTurnSpeed) AddStatRow($"Turn Speed:  {um.CurrentTurnSpeed:F1}");
        if (display.showMass)      AddStatRow($"Mass:        {um.CurrentMass:F2}");
        if (display.showGravity)   AddStatRow($"Gravity:     {def.baseGlideGravity:F1}");
        if (display.showMinSpeed)  AddStatRow($"Min Speed:   {def.baseMinSpeed:F1}");

        if (display.showAbilityStats && def.ability != null && um.AbilityLevel > 0)
        {
            var stats = def.ability.GetCurrentStats(def, um.AbilityLevel);
            foreach (var (label, value) in stats)
                AddStatRow($"{label}: {value}");
        }
    }

    private void AddStatRow(string text)
    {
        var go            = new GameObject("StatRow");
        go.transform.SetParent(statsContainer, false);
        var tmp           = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = 13;
        tmp.alignment     = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;
        var le            = go.AddComponent<LayoutElement>();
        le.preferredHeight = 20f;
    }
}