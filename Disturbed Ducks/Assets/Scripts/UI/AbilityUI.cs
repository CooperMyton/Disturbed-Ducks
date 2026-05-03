using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    public static AbilityUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject      abilityPanel;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private Image           cooldownFill;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private AbilityController abilityController;

    private float _cooldownTotal     = 1f;
    private float _cooldownRemaining = 0f;
    private bool  _onCooldown        = false;

    // Bomb-specific: counts down to detonation instead of showing a recharge cooldown.
    private bool  _isBombMode        = false;

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
        if (cooldownText != null) cooldownText.text = "READY";
        if (abilityPanel != null) abilityPanel.SetActive(true);
    }

    private void Update()
    {
        if (!_onCooldown && !_isBombMode) return;

        _cooldownRemaining -= Time.deltaTime;

        if (_cooldownRemaining <= 0f)
        {
            _cooldownRemaining = 0f;
            SetFill(_isBombMode ? 0f : 1f);

            if (_isBombMode)
            {
                // Duck is about to crash — brief confirmation before reset clears it
                if (cooldownText != null) cooldownText.text = "BOOM";
                _isBombMode = false;
            }
            else
            {
                _onCooldown = false;
                if (cooldownText != null) cooldownText.text = "READY";
            }
            return;
        }

        float ratio = _cooldownRemaining / _cooldownTotal;

        if (_isBombMode)
        {
            // Fill drains toward empty as detonation approaches
            SetFill(ratio);
            if (cooldownText != null) cooldownText.text = $"{_cooldownRemaining:F1}s";
        }
        else
        {
            // Normal cooldown: fill refills toward full as recharge completes
            SetFill(ratio);
            if (cooldownText != null) cooldownText.text = $"{_cooldownRemaining:F1}s";
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>Called by AbilityController for normal recharge abilities.</summary>
    public void OnAbilityUsed(float cooldownDuration)
    {
        _isBombMode        = false;
        _cooldownTotal     = cooldownDuration;
        _cooldownRemaining = cooldownDuration;
        _onCooldown        = true;

        if (cooldownText != null) cooldownText.text = $"{_cooldownRemaining:F1}s";
    }

    /// <summary>
    /// Called by ExplosionOnCrash when the bomb countdown starts.
    /// Replaces the normal cooldown display with a detonation timer.
    /// </summary>
    public void OnBombArmed(float detonationDelay)
    {
        _onCooldown        = false;
        _isBombMode        = true;
        _cooldownTotal     = detonationDelay;
        _cooldownRemaining = detonationDelay;

        SetFill(1f);
        if (cooldownText != null) cooldownText.text = $"{_cooldownRemaining:F1}s";
    }

    public void ResetCooldown()
    {
        _onCooldown        = false;
        _isBombMode        = false;
        _cooldownRemaining = 0f;
        SetFill(1f);

        if (cooldownText != null) cooldownText.text = "READY";
    }

    public void Show() { if (abilityPanel != null) abilityPanel.SetActive(true); }
    public void Hide() { if (abilityPanel != null) abilityPanel.SetActive(false); }

    private void SetFill(float ratio)
    {
        if (cooldownFill != null) cooldownFill.fillAmount = ratio;
    }
    public void RefreshName()
    {
        if (abilityNameText != null && abilityController != null)
            abilityNameText.text = abilityController.AbilityName;
    }
}