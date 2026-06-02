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
        EndOfAttemptUI.Instance?.SetEndRunVisible(!TitleScreenUI.IsShowing);
        upgradeUI?.Hide();
        EndOfAttemptUI.Instance?.Hide();
        LoadoutUI.Instance?.Show();
        SetPrompt("Select your duck and use WASD to set duck, X and Z to set launcher, and Space to launch");
    }

    public void OnLaunched()
    {
        EndOfAttemptUI.Instance?.SetEndRunVisible(true);
        TutorialManager.Instance?.OnLaunched(); // add this
        LoadoutUI.Instance?.Hide();
        SetPrompt("");
    }

    public void OnCrashed()
    {
        TutorialManager.Instance?.OnCrashed();

        bool hasRemaining = PlayerDuckInventory.Instance != null &&
                            PlayerDuckInventory.Instance.HasAnyRemaining();

        EndOfAttemptUI.Instance?.SetEndRunVisible(hasRemaining && !TitleScreenUI.IsShowing);

        if (!hasRemaining)
        {
            SetPrompt("");
            EndOfAttemptUI.Instance?.Show();
            upgradeUI?.Show();
        }
        else
        {
            SetPrompt("Press R for next duck");
        }
    }

    private void SetPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;
    }
    public void ShowFinalHeroPrompt()
    {
        EndOfAttemptUI.Instance?.SetEndRunVisible(false);
        upgradeUI?.Hide();
        EndOfAttemptUI.Instance?.Hide();
        LoadoutUI.Instance?.Hide();
        SetPrompt("Launch the Hero King Duck to finish the Mecha Husky");
    }
}