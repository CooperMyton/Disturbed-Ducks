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

    [Header("Tab System")]
    [SerializeField] private GameDefinition gameDefinition;
    [SerializeField] private Transform      tabContainer;
    [SerializeField] private Color tabSelectedColor   = Color.white;
    [SerializeField] private Color tabUnselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color tabUnownedColor    = new Color(0.35f, 0.35f, 0.35f, 1f);

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

    private DuckDefinition _selectedTab;

    private readonly List<(Button btn, Image img)> _tabData
        = new List<(Button, Image)>();

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
        // Auto-jump to flying duck's tab on open
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

        if (upgradeContent  != null) upgradeContent.SetActive(owned);
        if (purchaseContent != null) purchaseContent.SetActive(!owned);
        if (duckNameText    != null) duckNameText.text = _selectedTab.duckName;

        if (owned) RefreshUpgradeContent();
        else       RefreshPurchaseContent();
    }

    // -------------------------------------------------------------------------

    private void BuildTabs()
    {
        if (tabContainer == null || gameDefinition == null)
        {
            Debug.LogError($"BuildTabs early return — tabContainer null: {tabContainer == null}, gameDefinition null: {gameDefinition == null}");
            return;
        }

        foreach (Transform child in tabContainer) Destroy(child.gameObject);
        _tabData.Clear();

        foreach (var duck in gameDefinition.allDucks)
        {
            var go = new GameObject(duck.duckName + "_Tab");
            go.transform.SetParent(tabContainer, false);

            // FIX 1: LayoutElement tells the HLG the tab's preferred height so
            // Control Child Size Height gives it 36px instead of 0px (Image
            // with no sprite reports preferredHeight = 0).
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

            var tmp       = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = duck.duckName;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 14;
            // FIX 2: TMP raycastTarget is true by default; the label was
            // sitting on top of the Button and swallowing pointer events.
            tmp.raycastTarget = false;

            var capturedDuck = duck;
            btn.onClick.AddListener(() => SelectTab(capturedDuck));

            // activeSelf = true so the tab shows when UpgradePanel becomes active.
            // activeInHierarchy is still false here (panel is hidden at build time)
            // — that's expected and fine.
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
            var duck       = gameDefinition.allDucks[i];
            var (_, img)   = _tabData[i];
            bool isSelected = duck == _selectedTab;
            bool owned      = (PlayerDuckInventory.Instance?.GetOwned(duck) ?? 0) > 0;

            img.color = isSelected ? tabSelectedColor
                    : owned      ? tabUnselectedColor
                                : tabUnownedColor;
        }
    }

    private void RefreshUpgradeContent()
    {
        var um  = UpgradeManager.Instance;
        if (um == null) return;
        var def = _selectedTab;

        if (speedLevelText != null)
            speedLevelText.text =
                $"{def.maxSpeedUpgrade.upgradeName}: {um.SpeedLevel} / {def.maxSpeedUpgrade.levels.Length}";
        if (speedButton != null)     speedButton.interactable = um.CanUpgradeSpeed;
        if (speedButtonText != null) speedButtonText.text =
            GetStatLabel(um.CanUpgradeSpeed, um.SpeedLevel, def.maxSpeedUpgrade.levels);

        if (maneurLevelText != null)
            maneurLevelText.text =
                $"{def.manoeuvrabilityUpgrade.upgradeName}: {um.ManeurLevel} / {def.manoeuvrabilityUpgrade.levels.Length}";
        if (maneurButton != null)     maneurButton.interactable = um.CanUpgradeManeur;
        if (maneurButtonText != null) maneurButtonText.text =
            GetStatLabel(um.CanUpgradeManeur, um.ManeurLevel, def.manoeuvrabilityUpgrade.levels);

        bool locked = um.AbilityLevel == 0;
        if (abilityLevelText != null)
            abilityLevelText.text = locked
                ? $"{def.abilityUpgrade.upgradeName}: LOCKED"
                : $"{def.abilityUpgrade.upgradeName}: {um.AbilityLevel} / {def.abilityUpgrade.levels.Length}";
        if (abilityButton != null)     abilityButton.interactable = um.CanUpgradeAbility;
        if (abilityButtonText != null) abilityButtonText.text = GetAbilityLabel(um, def, locked);

        RefreshBuyDuck(def);
    }

    private void RefreshPurchaseContent()
    {
        if (_selectedTab == null) return;
        int  cost      = _selectedTab.GetPurchaseCost(0);
        bool canAfford = CurrencyManager.Instance?.CanAfford(cost) ?? false;

        if (purchaseTitleText != null)
            purchaseTitleText.text = $"Unlock {_selectedTab.duckName}";
        if (purchaseCostText != null)
            purchaseCostText.text = cost > 0 ? $"Cost: {cost} coins" : "Cost: Free";
        if (purchaseUnlockButton != null)
            purchaseUnlockButton.interactable = canAfford;
        if (purchaseUnlockButtonText != null)
            purchaseUnlockButtonText.text = canAfford ? "PURCHASE" : "NOT ENOUGH COINS";
    }

    private void RefreshBuyDuck(DuckDefinition def)
    {
        if (buyDuckButton == null || buyDuckButtonText == null) return;
        var  inv       = PlayerDuckInventory.Instance;
        if (inv == null) return;
        bool full      = inv.TotalOwned >= inv.MaxTotalDucks;
        int  cost      = def.GetPurchaseCost(inv.GetOwned(def));
        bool canAfford = CurrencyManager.Instance?.CanAfford(cost) ?? false;

        buyDuckButton.interactable = !full && canAfford;
        buyDuckButtonText.text = full      ? "LOADOUT FULL"
            : cost > 0 ? $"BUY DUCK ({cost} coins)" : "BUY DUCK (Free)";
    }

    // -------------------------------------------------------------------------

    private string GetStatLabel(bool canUpgrade, int currentLevel, StatUpgradeLevelData[] levels)
    {
        if (!canUpgrade) return "MAX";
        int cost = levels[currentLevel].cost;
        return cost > 0 ? $"UPGRADE ({cost} coins)" : "UPGRADE (Free)";
    }

    private string GetAbilityLabel(UpgradeManager um, DuckDefinition def, bool locked)
    {
        if (!um.CanUpgradeAbility) return "MAX";
        int    cost   = def.abilityUpgrade.levels[um.AbilityLevel].cost;
        string action = locked ? "UNLOCK" : "UPGRADE";
        return cost > 0 ? $"{action} ({cost} coins)" : $"{action} (Free)";
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
}