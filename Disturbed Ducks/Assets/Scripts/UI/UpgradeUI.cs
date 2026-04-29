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

    // -------------------------------------------------------------------------

    public static UpgradeUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Guard every button — if not wired up it won't crash
        speedButton? .onClick.AddListener(() => OnUpgradeClicked(0));
        maneurButton?.onClick.AddListener(() => OnUpgradeClicked(1));
        abilityButton?.onClick.AddListener(() => OnUpgradeClicked(2));
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

        // Duck name
        if (duckNameText != null)
            duckNameText.text = def.duckName;

        // Speed
        if (speedLevelText != null)
            speedLevelText.text =
                $"{def.maxSpeedUpgrade.upgradeName}: " +
                $"{um.SpeedLevel} / {def.maxSpeedUpgrade.maxLevels}";
        if (speedButton != null)
            speedButton.interactable = um.CanUpgradeSpeed;
        if (speedButtonText != null)
            speedButtonText.text = um.CanUpgradeSpeed ? "UPGRADE" : "MAX";

        // Manoeuvrability
        if (maneurLevelText != null)
            maneurLevelText.text =
                $"{def.manoeuvrabilityUpgrade.upgradeName}: " +
                $"{um.ManeurLevel} / {def.manoeuvrabilityUpgrade.maxLevels}";
        if (maneurButton != null)
            maneurButton.interactable = um.CanUpgradeManeur;
        if (maneurButtonText != null)
            maneurButtonText.text = um.CanUpgradeManeur ? "UPGRADE" : "MAX";

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
            abilityButtonText.text = abilityLocked
                ? "UNLOCK"
                : (um.CanUpgradeAbility ? "UPGRADE" : "MAX");
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
}