using UnityEngine;
using System.Collections;

/// <summary>
/// Add to DuckRoot. Handles Bomb Duck explosion on crash and ability explosion.
/// Safe on all ducks — does nothing if explosionDef is null (e.g. Basic Duck).
/// </summary>
public class ExplosionOnCrash : MonoBehaviour
{
    private ExplosionDefinition _def;
    private AbilityController   _abilityController;
    private DuckImpact          _duckImpact;
    private DuckModelController _modelController;

    // Per-flight state
    private bool      _explosionFired  = false;
    private Coroutine _pendingExplosion;
    private Coroutine _colorLerp;

    // Accumulated from ability upgrades — persists across resets, rebuilt by ApplyCurrentStats
    private float _radiusBoost    = 0f;
    private float _damageBoost    = 0f;
    private float _delayReduction = 0f;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _abilityController = GetComponent<AbilityController>();
        _duckImpact        = GetComponent<DuckImpact>();
        _modelController   = GetComponent<DuckModelController>();
    }

    public void SetExplosionDefinition(ExplosionDefinition def) => _def = def;

    public void ResetUpgrades()
    {
        _radiusBoost    = 0f;
        _damageBoost    = 0f;
        _delayReduction = 0f;
    }

    public void ApplyUpgrade(float radius, float damage, float delay)
    {
        _radiusBoost    += radius;
        _damageBoost    += damage;
        _delayReduction += delay;
    }

    // -------------------------------------------------------------------------

    // Called by BombExplosionAbility.Use() when Shift is pressed
    public void StartAbilityCountdown(float radiusBoost, float damageBoost, float delayReduction)
    {
        if (_def == null)              return;
        if (_pendingExplosion != null)  return; // already armed — no re-trigger

        float delay  = Mathf.Max(0.05f, _def.abilityDelay  - delayReduction);
        float radius = _def.abilityRadius + radiusBoost;
        float damage = _def.abilityDamage + damageBoost;

        // Tell AbilityUI to show a detonation countdown instead of a recharge cooldown
        AbilityUI.Instance?.OnBombArmed(delay);

        // Start color lerp toward red over the countdown duration
        if (_colorLerp != null) StopCoroutine(_colorLerp);
        _colorLerp = StartCoroutine(LerpToRedRoutine(delay));

        _pendingExplosion = StartCoroutine(AbilityCountdownRoutine(radius, damage, delay));
    }

    // Called by DuckController.OnCrashed()
    public void OnCrashed()
    {
        if (_def == null)    return;
        if (_explosionFired) return;

        // Cancel any pending ability explosion — crash fires its own
        if (_pendingExplosion != null)
        {
            StopCoroutine(_pendingExplosion);
            _pendingExplosion = null;
        }

        if (_colorLerp != null)
        {
            StopCoroutine(_colorLerp);
            _colorLerp = null;
        }

        bool useAbility = _abilityController != null && _abilityController.IsUnlocked;

        if (useAbility)
            TriggerExplosion(_def.abilityRadius + _radiusBoost,
                             _def.abilityDamage + _damageBoost);
        else
            TriggerExplosion(_def.baseRadius, _def.baseDamage);
    }

    // Called by DuckController.OnReset()
    public void ResetFlight()
    {
        _explosionFired = false;

        if (_pendingExplosion != null)
        {
            StopCoroutine(_pendingExplosion);
            _pendingExplosion = null;
        }

        if (_colorLerp != null)
        {
            StopCoroutine(_colorLerp);
            _colorLerp = null;
        }

        // Restore original model color
        _modelController?.ClearTint();
    }

    // -------------------------------------------------------------------------

    private IEnumerator LerpToRedRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // White → red: green and blue channels drop to 0
            _modelController?.SetTint(new Color(1f, 1f - t, 1f - t, 1f));
            yield return null;
        }
        _modelController?.SetTint(Color.red);
        _colorLerp = null;
    }

    private IEnumerator AbilityCountdownRoutine(float radius, float damage, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!_explosionFired)
        {
            TriggerExplosion(radius, damage);
            _duckImpact?.Crash();
        }

        _pendingExplosion = null;
    }

    private void TriggerExplosion(float radius, float damage)
    {
        _explosionFired = true;
        ExplosionHelper.Trigger(transform.position, radius, damage, _def, this);
    }
}