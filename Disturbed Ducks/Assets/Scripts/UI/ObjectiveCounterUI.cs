using UnityEngine;
using TMPro;

public class ObjectiveCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    private void OnEnable()  => StageManager.OnObjectivesChanged += UpdateCounter;
    private void OnDisable() => StageManager.OnObjectivesChanged -= UpdateCounter;

    private void UpdateCounter(int remaining, int total)
    {
        if (counterText != null)
            counterText.text = $"Objectives: {total - remaining} / {total}";
    }
}