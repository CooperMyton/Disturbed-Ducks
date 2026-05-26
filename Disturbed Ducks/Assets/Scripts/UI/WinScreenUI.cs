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
    private string winMessage = "YOU WIN!\nThe King Duck sacrificed himself to take down the mecha and Husky Inc.\nHusky and Beaver Inc are gone for good! The Willamette River is free!";

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
        Show(winMessage);
    }

    public void Show(string message)
    {
        panel?.SetActive(true);
        if (messageText != null) messageText.text = message;
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