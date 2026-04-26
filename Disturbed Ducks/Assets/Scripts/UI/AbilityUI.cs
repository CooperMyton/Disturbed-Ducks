using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// Displays ability name and cooldown radial fill.
/// Singleton so AbilityController can reach it without a direct reference.

public class AbilityUI : MonoBehaviour
{
    public static AbilityUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject abilityPanel;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private Image cooldownFill;       // radial fill image
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("References")]
    [SerializeField] private AbilityController abilityController;

    private float _cooldownTotal;
    private float _cooldownRemaining;
    private bool _onCooldown = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        //if (abilityController != null)
            //abilityNameText.text = abilityController.AbilityName;

        SetFill(1f);
        cooldownText.text = "";
    }

    private void Update()
    {
        if (!_onCooldown) return;

        _cooldownRemaining -= Time.deltaTime;

        if (_cooldownRemaining <= 0f)
        {
            _onCooldown = false;
            SetFill(1f);
            cooldownText.text = "READY";
            return;
        }

        float ratio = _cooldownRemaining / _cooldownTotal;
        SetFill(ratio);
        cooldownText.text = $"{_cooldownRemaining:F1}s";
    }

    // -------------------------------------------------------------------------

    public void OnAbilityUsed(float cooldownDuration)
    {
        _cooldownTotal = cooldownDuration;
        _cooldownRemaining = cooldownDuration;
        _onCooldown = true;
        cooldownText.text = $"{_cooldownRemaining:F1}s";
    }

    public void Show() => abilityPanel.SetActive(true);
    public void Hide() => abilityPanel.SetActive(false);

    private void SetFill(float ratio)
    {
        if (cooldownFill != null)
            cooldownFill.fillAmount = ratio;
    }
}