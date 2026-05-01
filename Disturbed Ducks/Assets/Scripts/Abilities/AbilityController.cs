using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key abilityKey = Key.LeftShift;

    private AbilityBase _ability;
    private float _cooldownTimer = 0f;
    private bool _isFlying = false;
    private bool _isUnlocked = false;

    // Accumulated from upgrade levels
    private float _upgradeBoost = 0f;
    private float _cooldownReduction = 0f;

    public float CooldownRemaining  => Mathf.Max(_cooldownTimer, 0f);
    public float CurrentCooldown    => _ability != null
        ? Mathf.Max(_ability.cooldown - _cooldownReduction, 0.5f)
        : 1f;
    public bool  IsReady            => _cooldownTimer <= 0f && _ability != null && _isUnlocked;
    public bool  IsUnlocked         => _isUnlocked;
    public string AbilityName       => _ability != null ? _ability.abilityName : "";

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (Keyboard.current[abilityKey].wasPressedThisFrame)
        {
            //Debug.Log($"Shift pressed — isFlying: {_isFlying}, isUnlocked: {_isUnlocked}, " +
            //        $"ability null: {_ability == null}, cooldown: {_cooldownTimer:F2}, isReady: {IsReady}");

            // Only trigger if actually flying
            if (IsReady && _isFlying)
                TriggerAbility();
        }
    }

    // -------------------------------------------------------------------------

    public void SetAbility(AbilityBase ability)
    {
        _ability = ability;
    }

    public void UnlockAbility()
    {
        _isUnlocked = true;
    }

    /// <summary>
    /// Called by UpgradeManager for each ability upgrade level purchased.
    /// </summary>
    public void ApplyAbilityUpgrade(float boostIncrement, float cooldownReduction)
    {
        _upgradeBoost      += boostIncrement;
        _cooldownReduction += cooldownReduction;
    }

    public void OnLaunched()  => _isFlying = true;
    public void OnCrashed()   => _isFlying = false;
    public void OnReset()
    {
        _isFlying = false;
        _cooldownTimer = 0f;
        AbilityUI.Instance?.ResetCooldown();
    }

    // -------------------------------------------------------------------------

    private void TriggerAbility()
    {
        //Debug.Log($"TriggerAbility — cooldown will be: {CurrentCooldown:F2}");
        float actualCooldown = CurrentCooldown;
        _ability.Use(gameObject, _upgradeBoost);
        _cooldownTimer = actualCooldown;
        //Debug.Log($"TriggerAbility — cooldownTimer set to: {_cooldownTimer:F2}");

        GetComponent<DuckController>()?.OnAbilityUsed();
        AbilityUI.Instance?.OnAbilityUsed(actualCooldown);
    }
}