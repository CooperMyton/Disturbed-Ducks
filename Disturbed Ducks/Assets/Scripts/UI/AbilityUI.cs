using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    public static AbilityUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject        abilityPanel;
    [SerializeField] private TextMeshProUGUI   abilityNameText;
    [SerializeField] private Image             abilityIndicator;   // yellow/grey square
    [SerializeField] private TextMeshProUGUI   statusText;         // READY / USED / countdown
    [SerializeField] private AbilityController abilityController;

    [Header("Colors")]
    [SerializeField] private Color readyColor  = Color.yellow;
    [SerializeField] private Color usedColor   = new Color(0.4f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private float _countdownRemaining = 0f;

    private string _countdownCompleteText = "BOOM";

    private bool  _isCountingDown     = false;

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
        RefreshName();
        RefreshIndicator();
        if (abilityPanel != null) abilityPanel.SetActive(true);
    }

    private void Update()
    {
        if (!_isCountingDown) return;

        _countdownRemaining -= Time.deltaTime;

        if (_countdownRemaining <= 0f)
        {
            _countdownRemaining = 0f;
            _isCountingDown     = false;
            SetIndicator(usedColor, _countdownCompleteText);

            return;
        }

        if (statusText != null) statusText.text = $"{_countdownRemaining:F1}s";
    }

    // -------------------------------------------------------------------------

    /// Called for all single-use abilities (split, dash etc) after firing.
    public void OnAbilityUsed(float _)
    {
        if (_isCountingDown) return; // don't override bomb/anvil countdown
        SetIndicator(usedColor, "USED");
    }

    /// Called by ExplosionOnCrash (bomb) and AnvilSlamController.
    /// Replaces the indicator with a live countdown.
    public void OnBombArmed(float duration)
    {
        StartCountdown(duration, "BOOM");
    }

    public void OnPhaseStarted(float duration)
    {
        StartCountdown(duration, "USED");
    }

    private void StartCountdown(float duration, string completeText)
    {
        _isCountingDown = true;
        _countdownRemaining = duration;
        _countdownCompleteText = completeText;
        SetIndicator(readyColor, $"{duration:F1}s");
    }


    public void ResetCooldown()
    {
        _isCountingDown     = false;
        _countdownRemaining = 0f;
        RefreshIndicator();
    }

    public void RefreshName()
    {
        if (abilityNameText != null && abilityController != null)
        {
            abilityNameText.text = abilityController.AbilityInputEnabled
                ? abilityController.AbilityName
                : "No Ability";
        }

        RefreshIndicator();
    }

    public void Show() { if (abilityPanel != null) abilityPanel.SetActive(true); }
    public void Hide() { if (abilityPanel != null) abilityPanel.SetActive(false); }

    // -------------------------------------------------------------------------

    private void RefreshIndicator()
    {
        if (abilityController == null) return;
        
        if (!abilityController.AbilityInputEnabled)
        {
            SetIndicator(lockedColor, "NO ABILITY");
            return;
        }
        
        if (!abilityController.IsUnlocked)
            SetIndicator(lockedColor, "Upgrade to Unlock");
        else if (abilityController.IsReady)
            SetIndicator(readyColor, "READY");
        else
            SetIndicator(usedColor, "USED");
    }

    private void SetIndicator(Color color, string text)
    {
        if (abilityIndicator != null) abilityIndicator.color = color;
        if (statusText       != null) statusText.text        = text;
    }
    public void SetAmmo(int currentAmmo, int maxAmmo)
    {
        _isCountingDown = false;

        if (currentAmmo > 0)
            SetIndicator(readyColor, $"AMMO {currentAmmo}/{maxAmmo}");
        else
            SetIndicator(usedColor, "NO AMMO");
    }
}