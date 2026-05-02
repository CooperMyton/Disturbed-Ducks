using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AbilityUI : MonoBehaviour
{
    public static AbilityUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject abilityPanel;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private AbilityController abilityController;

    private float _cooldownTotal = 1f;
    private float _cooldownRemaining = 0f;
    private bool _onCooldown = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(InitDisplay), 0.2f);
    }

    private void InitDisplay()
    {
        if (abilityController != null && abilityNameText != null)
            abilityNameText.text = abilityController.AbilityName;

        SetFill(1f);

        if (cooldownText != null)
            cooldownText.text = "READY";

        if (abilityPanel != null)
            abilityPanel.SetActive(true);
    }

    private void Update()
    {

        _cooldownRemaining -= Time.deltaTime;

        if (_cooldownRemaining <= 0f)
        {
            _onCooldown = false;
            _cooldownRemaining = 0f;
            SetFill(1f);
            if (cooldownText != null)
                cooldownText.text = "READY";
            return;
        }

        float ratio = _cooldownRemaining / _cooldownTotal;
        SetFill(ratio);

        if (cooldownText != null)
            cooldownText.text = $"{_cooldownRemaining:F1}s";
    }

    // -------------------------------------------------------------------------

    public void OnAbilityUsed(float cooldownDuration)
    {
        Debug.Log($"AbilityUI.OnAbilityUsed called — duration: {cooldownDuration}");
        _cooldownTotal     = cooldownDuration;
        _cooldownRemaining = cooldownDuration;
        _onCooldown        = true;

        if (cooldownText != null)
            cooldownText.text = $"{_cooldownRemaining:F1}s";
    }

    public void ResetCooldown()
    {
        Debug.Log($"AbilityUI.ResetCooldown called from: {System.Environment.StackTrace}");

        _onCooldown        = false;
        _cooldownRemaining = 0f;
        SetFill(1f);

        if (cooldownText != null)
            cooldownText.text = "READY";
    }

    public void Show() { if (abilityPanel != null) abilityPanel.SetActive(true); }
    public void Hide() { if (abilityPanel != null) abilityPanel.SetActive(false); }

    private void SetFill(float ratio)
    {
        if (cooldownFill != null)
            cooldownFill.fillAmount = ratio;
    }
    

}