using UnityEngine;
using UnityEngine.InputSystem;


/// Sits on DuckRoot. Holds the duck's current ability and fires it on input.
/// Swap the ability asset to change what the duck can do — no code changes needed.

public class AbilityController : MonoBehaviour
{
    [Header("Ability")]
    [SerializeField] private AbilityBase ability;

    [Header("Input")]
    [SerializeField] private Key abilityKey = Key.LeftShift;

    private float _cooldownTimer = 0f;
    private bool _isFlying = false;

    // Read by AbilityUI
    public float CooldownRemaining => Mathf.Max(_cooldownTimer, 0f);
    public float CooldownTotal     => ability != null ? ability.cooldown : 1f;
    public bool  IsReady           => _cooldownTimer <= 0f && ability != null;
    public string AbilityName      => ability != null ? ability.abilityName : "";

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        if (!_isFlying) return;

        if (Keyboard.current[abilityKey].wasPressedThisFrame && IsReady)
            TriggerAbility();
    }

    // -------------------------------------------------------------------------

    public void OnLaunched()  => _isFlying = true;
    public void OnCrashed()   => _isFlying = false;
    public void OnReset()
    {
        _isFlying = false;
        _cooldownTimer = 0f;
    }

    // -------------------------------------------------------------------------

    private void TriggerAbility()
    {
        ability.Use(gameObject);
        _cooldownTimer = ability.cooldown;
        AbilityUI.Instance?.OnAbilityUsed(ability.cooldown);
        Debug.Log($"Used ability: {ability.abilityName}");
    }
}