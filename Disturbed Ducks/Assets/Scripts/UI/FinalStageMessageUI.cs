using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalStageMessageUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    private System.Action _onClicked;

    private void Awake()
    {
        button?.onClick.AddListener(HandleClick);
        panel?.SetActive(false);
    }

    public void Show(string message, string label, System.Action onClicked)
    {
        _onClicked = onClicked;
        if (messageText != null) messageText.text = message;
        if (buttonText != null) buttonText.text = label;
        panel?.SetActive(true);
    }

    private void HandleClick()
    {
        panel?.SetActive(false);
        _onClicked?.Invoke();
        _onClicked = null;
    }
}