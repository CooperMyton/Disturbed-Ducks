using UnityEngine;
using TMPro;

public class FlightUIManager : MonoBehaviour
{
    public static FlightUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private UpgradeUI upgradeUI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ShowLaunchPrompt();
        Invoke(nameof(InitializeUI), 0.5f);
    }

    private void InitializeUI()
    {
        AbilityUI.Instance?.Show();
        ShowLaunchPrompt();
    }

    public void ShowLaunchPrompt()
    {
        upgradeUI?.Hide();
        EndOfAttemptUI.Instance?.Hide();
        LoadoutUI.Instance?.Show();
        SetPrompt("Select duck and use WASD to aim, Space to launch");
    }

    public void OnLaunched()
    {
        LoadoutUI.Instance?.Hide();
        SetPrompt("");
    }

    public void OnCrashed()
    {
        bool hasRemaining = PlayerDuckInventory.Instance != null &&
                            PlayerDuckInventory.Instance.HasAnyRemaining();

        if (!hasRemaining)
        {
            // Out of birds — show end screen AND upgrade panel so they can
            // spend coins before restarting
            SetPrompt("");
            EndOfAttemptUI.Instance?.Show();
            upgradeUI?.Show();
        }
        else
        {
            // Birds still available — just prompt, no upgrade menu
            SetPrompt("Press R for next duck");
        }
    }

    private void SetPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }
}