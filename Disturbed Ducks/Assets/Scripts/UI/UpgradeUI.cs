using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the upgrade panel on crash.
/// Pip indicators are generated at runtime — no manual setup needed per level.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI duckNameText;

    [Header("Speed Row")]
    [SerializeField] private Transform tierContainer;         // HorizontalLayoutGroup lives here
    [SerializeField] private TextMeshProUGUI levelText;       // "Level 3 / 10"
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    [Header("Pip Appearance")]
    [SerializeField] private Color filledColor  = new Color(1f, 0.85f, 0f);   // gold
    [SerializeField] private Color emptyColor   = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private float pipSize      = 24f;
    [SerializeField] private float pipSpacing   = 6f;

    private Image[] _pips;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        Hide();
    }

    private void Start()
    {
        BuildPips();
        Refresh();
    }

    // -------------------------------------------------------------------------

    public void Show()
    {
        upgradePanel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        upgradePanel.SetActive(false);
    }

    // -------------------------------------------------------------------------

    private void BuildPips()
    {
        if (UpgradeManager.Instance == null) return;

        int max = UpgradeManager.Instance.MaxSpeedLevel;
        _pips = new Image[max];

        // Get or add a layout group so pips space themselves automatically
        HorizontalLayoutGroup layout = tierContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) layout = tierContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = pipSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < max; i++)
        {
            GameObject pip = new GameObject($"Pip_{i + 1}", typeof(RectTransform), typeof(Image));
            pip.transform.SetParent(tierContainer, false);

            RectTransform rt = pip.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(pipSize, pipSize);

            _pips[i] = pip.GetComponent<Image>();
        }
    }

    private void Refresh()
    {
        if (UpgradeManager.Instance == null) return;

        int current = UpgradeManager.Instance.SpeedLevel;
        int max     = UpgradeManager.Instance.MaxSpeedLevel;
        bool canUpgrade = UpgradeManager.Instance.CanUpgradeSpeed;

        // Duck name
        if (duckNameText != null)
            duckNameText.text = UpgradeManager.Instance.DuckName;

        // Fill pips up to current level
        if (_pips != null)
            for (int i = 0; i < _pips.Length; i++)
                _pips[i].color = i < current ? filledColor : emptyColor;

        // Level label
        if (levelText != null)
            levelText.text = $"Level {current} / {max}";

        // Button state
        upgradeButton.interactable = canUpgrade;
        upgradeButtonText.text = canUpgrade ? "UPGRADE" : "MAX";
    }

    private void OnUpgradeClicked()
    {
        if (UpgradeManager.Instance == null) return;
        UpgradeManager.Instance.TryUpgradeSpeed();
        Refresh();
    }
}