using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    private static bool _hasStartedThisSession;

    [SerializeField] private GameObject titlePanel;
    [SerializeField] private Button startButton;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private bool resetProgressOnStart = true;

    public static bool IsShowing { get; private set; }

    private void Awake()
    {
        startButton?.onClick.AddListener(StartGame);

        if (_hasStartedThisSession)
        {
            IsShowing = false;
            titlePanel?.SetActive(false);
            EndOfAttemptUI.Instance?.SetEndRunVisible(false);
            AudioListener.pause = false;
            Time.timeScale = 1f;
            return;
        }

        IsShowing = true;
        titlePanel?.SetActive(true);
        EndOfAttemptUI.Instance?.SetEndRunVisible(false);
        AudioListener.pause = true;
        Time.timeScale = 0f;
    }

    private void StartGame()
    {
        IsShowing = false;
        _hasStartedThisSession = true;

        AudioListener.pause = false;
        Time.timeScale = 1f;

        if (resetProgressOnStart)
            inventory?.ResetAllProgress();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void Start()
    {
        if (IsShowing)
            EndOfAttemptUI.Instance?.SetEndRunVisible(false);
    }
}