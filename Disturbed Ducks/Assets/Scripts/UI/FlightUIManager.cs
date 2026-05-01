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
        SetPrompt("Press R for next duck");

        if (PlayerDuckInventory.Instance != null &&
            !PlayerDuckInventory.Instance.HasAnyRemaining())
        {
            EndOfAttemptUI.Instance?.Show();
        }
        else
        {
            upgradeUI?.Show();
        }
    }

    private void SetPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }
}