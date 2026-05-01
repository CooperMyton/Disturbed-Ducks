using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// Shown when all ducks are used.
/// Displays upgrade panel and restart option.
/// </summary>
public class EndOfAttemptUI : MonoBehaviour
{
    public static EndOfAttemptUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Button restartButton;
    [SerializeField] private DuckSpawner duckSpawner;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        restartButton?.onClick.AddListener(OnRestartClicked);
        panel?.SetActive(false);
    }

    private void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged += UpdateCurrencyDisplay;
    }

    // -------------------------------------------------------------------------

    public void Show()
    {
        panel.SetActive(true);
        UpdateCurrencyDisplay(CurrencyManager.Instance?.Balance ?? 0);
        UpgradeUI.Instance?.Show();
        LoadoutUI.Instance?.Hide();
    }

    public void Hide()
    {
        panel.SetActive(false);
        UpgradeUI.Instance?.Hide();
    }

    // -------------------------------------------------------------------------

    private void UpdateCurrencyDisplay(int balance)
    {
        if (currencyText != null)
            currencyText.text = $"Currency: {balance}";
    }

    private void OnRestartClicked()
    {
        Hide();
        duckSpawner.RestartAttempt();
        LoadoutUI.Instance?.Show();
    }

}
