using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageClearUI : MonoBehaviour
{
    public static StageClearUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject      panel;
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private TextMeshProUGUI firstClearText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Button          nextStageButton;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        nextStageButton?.onClick.AddListener(OnNextStageClicked);
        panel?.SetActive(false);
    }

    // -------------------------------------------------------------------------

    public void Show(StageDefinition stage, bool isFirstClear)
    {
        EndOfAttemptUI.Instance?.SetEndRunVisible(true);
        panel.SetActive(true);

        if (stageNameText != null)
            stageNameText.text = $"{stage.stageName} Cleared!";

        if (firstClearText != null)
        {
            firstClearText.gameObject.SetActive(isFirstClear);
            firstClearText.text = $"First Clear Bonus! +{stage.firstClearBonus}";
        }

        if (currencyText != null)
            currencyText.text = $"Currency: {CurrencyManager.Instance?.Balance}";

        // Hide the next stage button if this is the final stage
        if (nextStageButton != null)
            nextStageButton.gameObject.SetActive(stage.nextStage != null);
    }

    public void Hide() => panel?.SetActive(false);

    // -------------------------------------------------------------------------

    private void OnNextStageClicked()
    {
        Hide();
        EndOfAttemptUI.Instance?.SetEndRunVisible(false);
        StageManager.Instance?.LoadNextStage();
    }
}