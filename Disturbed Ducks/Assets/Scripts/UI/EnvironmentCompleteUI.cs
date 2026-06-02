using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnvironmentCompleteUI : MonoBehaviour
{
    public static EnvironmentCompleteUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button nextLevelButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
        panel?.SetActive(false);
    }

    public void Show(string message)
    {
        EndOfAttemptUI.Instance?.SetEndRunVisible(false);
        if (messageText != null) messageText.text = message;
        panel?.SetActive(true);
    }

    public void Hide() => panel?.SetActive(false);

    private void OnNextLevelClicked()
    {
        EndOfAttemptUI.Instance?.SetEndRunVisible(false);
        Hide();
        StageManager.Instance?.LoadNextStage();
    }
}
