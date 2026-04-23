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

    private void Start() => ShowLaunchPrompt();

    public void ShowLaunchPrompt()
    {
        upgradeUI?.Hide();
        SetPrompt("Use WASD / Arrow Keys to launch");
    }

    public void OnLaunched() => SetPrompt("");

    public void OnCrashed()
    {
        SetPrompt("Press R to Respawn");
        upgradeUI?.Show();
    }

    private void SetPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }
}