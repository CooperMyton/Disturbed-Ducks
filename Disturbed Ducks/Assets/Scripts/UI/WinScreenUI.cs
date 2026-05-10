using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinScreenUI : MonoBehaviour
{
    public static WinScreenUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button          restartButton;
    [SerializeField] private AudioSource     jingleSource;
    [SerializeField] private ParticleSystem  confettiParticles;

    [Header("Settings")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] [TextArea(2, 5)]
    private string winMessage = "YOU WIN!\nBeaver Inc has been taken down!\nThe Willamette River is free!";

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        restartButton?.onClick.AddListener(OnRestartClicked);
        panel?.SetActive(false);
    }

    public void Show()
    {
        panel?.SetActive(true);
        if (messageText != null) messageText.text = winMessage;
        jingleSource?.Play();
        confettiParticles?.Play();
    }

    public void Hide() => panel?.SetActive(false);

    private void OnRestartClicked()
    {
        inventory?.ResetAllProgress();
        Hide();
        StageManager.RestartCurrentStage();
    }
}