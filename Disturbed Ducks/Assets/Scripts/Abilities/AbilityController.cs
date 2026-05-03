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

    private float _radiusBoost    = 0f;
    private float _damageBoost    = 0f;
    private float _delayReduction = 0f;

    public float CooldownRemaining  => Mathf.Max(_cooldownTimer, 0f);
    public float CurrentCooldown    => _ability != null
        ? Mathf.Max(_ability.cooldown - _cooldownReduction, 0.5f)
        : 1f;
    public bool  IsReady            => _cooldownTimer <= 0f && _ability != null && _isUnlocked;
    public bool  IsUnlocked         => _isUnlocked;

    public float RadiusBoost    => _radiusBoost;
    public float DamageBoost    => _damageBoost;
    public float DelayReduction => _delayReduction;
    public string AbilityName       => _ability != null ? _ability.abilityName : "";

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (Keyboard.current[abilityKey].wasPressedThisFrame)
        {
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

    public void ApplyAbilityUpgrade(float boostIncrement, float cooldownReduction,
        float radiusIncrement = 0f, float damageIncrement = 0f,
        float explosionDelayReduction = 0f)
    {
        _upgradeBoost      += boostIncrement;
        _cooldownReduction += cooldownReduction;
        _radiusBoost       += radiusIncrement;
        _damageBoost       += damageIncrement;
        _delayReduction    += explosionDelayReduction;

        GetComponent<ExplosionOnCrash>()?.ApplyUpgrade(
            radiusIncrement, damageIncrement, explosionDelayReduction);
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
        float actualCooldown = CurrentCooldown;
        _ability.Use(gameObject, _upgradeBoost);
        _cooldownTimer = actualCooldown;

        GetComponent<DuckController>()?.OnAbilityUsed();

        // Single-use abilities (e.g. bomb) skip the normal cooldown UI.
        // They notify AbilityUI directly (e.g. OnBombArmed) from within Use().
        if (!_ability.IsSingleUse)
            AbilityUI.Instance?.OnAbilityUsed(actualCooldown);
    }

    public void SetAbilityUpgrades(float totalBoost, float totalCooldown,
        float totalRadius = 0f, float totalDamage = 0f, float totalDelay = 0f)
    {
        _upgradeBoost      = totalBoost;
        _cooldownReduction = totalCooldown;
        _radiusBoost       = totalRadius;
        _damageBoost       = totalDamage;
        _delayReduction    = totalDelay;

        var exp = GetComponent<ExplosionOnCrash>();
        if (exp != null) { exp.ResetUpgrades(); exp.ApplyUpgrade(totalRadius, totalDamage, totalDelay); }
    }
    public void LockAbility()
    {
        _isUnlocked = false;
    }
}