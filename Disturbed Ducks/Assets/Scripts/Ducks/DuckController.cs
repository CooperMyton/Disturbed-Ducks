using UnityEngine;


/// Central hub for a duck — owns the DuckDefinition and coordinates
/// all subsystems (flight, sound, model, ability).
/// Swapping duck type = dragging a different DuckDefinition asset in.

public class DuckController : MonoBehaviour
{
    [SerializeField] private DuckDefinition definition;

    private DuckFlightController _flightController;
    private DuckSoundController _soundController;
    private DuckModelController _modelController;
    private AbilityController _abilityController;
    private FlightTrail _flightTrail;

    public DuckDefinition Definition => definition;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _flightController  = GetComponent<DuckFlightController>();
        _soundController   = GetComponent<DuckSoundController>();
        _modelController   = GetComponent<DuckModelController>();
        _abilityController = GetComponent<AbilityController>();
        _flightTrail       = GetComponentInChildren<FlightTrail>();

        ApplyDefinition();
    }

    /// <summary>
    /// Pushes all base stats from the definition to subsystems.
    /// Called on Awake and whenever the definition changes.
    /// </summary>
    public void ApplyDefinition()
    {
        if (definition == null) return;

        _flightController?.SetBaseStats(
            definition.baseMaxSpeed,
            definition.baseTurnSpeed,
            definition.baseGlideGravity,
            definition.baseMinSpeed
        );

        // Apply base mass from definition
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.mass = definition.baseMass;

        _soundController?.SetDefinition(definition);
        _modelController?.SetDefinition(definition);
        _abilityController?.SetAbility(definition.ability);
        _abilityController?.SetAbilityInputEnabled(!definition.disableAbilityInput);
        _flightController?.SetDisableInput(definition.disableFlightControls);
        GetComponent<ExplosionOnCrash>()?.SetExplosionDefinition(definition.explosionDefinition);
    }

    // -------------------------------------------------------------------------
    // Events — called by other components on this duck

    public void OnLaunched()
    {
        _abilityController?.OnLaunched();
        _soundController?.PlayLaunch();
        _modelController?.SetFlight();
        _flightTrail?.SetEmitting(true);
        GetComponent<DuckFamilyController>()?.OnLaunched();
    }

    public void OnCrashed()
    {
        GetComponent<ExplosionOnCrash>()?.OnCrashed();
        GetComponent<ShieldController>()?.ForceEnd();
        _abilityController?.OnCrashed();
        _soundController?.PlayCrash();
        _modelController?.SetCrashed();
        _flightTrail?.SetEmitting(false);
        GetComponent<DuckFamilyController>()?.OnLeaderCrashed();
    }

public void OnReset()
{
    _abilityController?.OnReset();
    GetComponent<ExplosionOnCrash>()?.ResetFlight();
    GetComponent<SplitController>()?.OnReset(); // clean up mini ducks
    GetComponent<ShieldController>()?.ForceEnd();
    GetComponent<DuckFamilyController>()?.OnReset();

    _modelController?.SetNeutral();
    _flightTrail?.Clear();
    _flightTrail?.SetEmitting(false);
}

    public void OnAbilityUsed()
    {
        _soundController?.PlayAbility();
    }
    public void ApplyDefinitionFromType(DuckDefinition newDefinition)
    {
        definition = newDefinition;
        ApplyDefinition();
    }
}