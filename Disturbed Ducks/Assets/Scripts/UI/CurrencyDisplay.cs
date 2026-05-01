using UnityEngine;
using TMPro;

public class CurrencyDisplay : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged += UpdateDisplay;

        Invoke(nameof(InitDisplay), 0.2f);
    }

    private void InitDisplay()
    {
        UpdateDisplay(CurrencyManager.Instance?.Balance ?? 0);
    }

    private void UpdateDisplay(int balance)
    {
        if (_text != null)
            _text.text = $"Currency: {balance}";
    }
}