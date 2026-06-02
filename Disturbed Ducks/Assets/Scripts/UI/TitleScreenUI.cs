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

    private void Awake()
    {
        startButton?.onClick.AddListener(StartGame);

        if (_hasStartedThisSession)
        {
            titlePanel?.SetActive(false);
            AudioListener.pause = false;
            Time.timeScale = 1f;
            return;
        }

        titlePanel?.SetActive(true);
        AudioListener.pause = true;
        Time.timeScale = 0f;
    }

    private void StartGame()
    {
        _hasStartedThisSession = true;

        AudioListener.pause = false;
        Time.timeScale = 1f;

        if (resetProgressOnStart)
            inventory?.ResetAllProgress();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}