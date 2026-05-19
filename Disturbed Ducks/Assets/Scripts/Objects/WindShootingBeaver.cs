using UnityEngine;

public class WindShootingBeaver : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private float range = 18f;
    [SerializeField] private float coneAngle = 28f;
    [SerializeField] private float windSpeed = 12f;
    [SerializeField] private AnimationCurve falloff = AnimationCurve.Linear(0f, 1f, 1f, 0.35f);

    [Header("Timing")]
    [SerializeField] private bool onlyAfterDuckLaunch = true;

    [Header("Aiming")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private float facingYawOffset = 180f;

    [Header("Visual")]
    [SerializeField] private ParticleSystem windParticles;

    private Transform _duck;
    private DuckFlightController _duckFlight;
    private DuckImpact _duckImpact;
    private bool _isActive = true;

    private void Start()
    {
        GameObject duckRoot = GameObject.FindGameObjectWithTag("Duck");
        if (duckRoot != null)
        {
            _duck = duckRoot.transform;
            _duckFlight = duckRoot.GetComponent<DuckFlightController>();
            _duckImpact = duckRoot.GetComponent<DuckImpact>();
        }

        TargetEnemy enemy = GetComponent<TargetEnemy>();
        if (enemy != null)
            enemy.OnDamaged += StopShooting;
    }

    private void Update()
    {
        bool shouldBlow = ShouldBlowWind();

        if (windParticles != null)
        {
            if (shouldBlow && !windParticles.isPlaying)
                windParticles.Play();
            else if (!shouldBlow && windParticles.isPlaying)
                windParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (!shouldBlow)
            return;

        AimAtDuck();
    }

    private void FixedUpdate()
    {
        if (!ShouldBlowWind())
            return;

        ApplyWindCone();
    }

    private bool ShouldBlowWind()
    {
        if (!_isActive) return false;
        if (_duck == null || _duckFlight == null) return false;
        if (onlyAfterDuckLaunch && !_duckFlight.IsLaunched) return false;
        if (_duckImpact != null && _duckImpact.HasCrashed) return false;

        return true;
    }

    private void AimAtDuck()
    {
        Vector3 direction = _duck.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up) *
            Quaternion.Euler(0f, facingYawOffset, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void ApplyWindCone()
    {
        Transform origin = firePoint != null ? firePoint : transform;

        Collider[] hits = Physics.OverlapSphere(origin.position, range);

        foreach (Collider hit in hits)
        {
            DuckFlightController flight = hit.GetComponentInParent<DuckFlightController>();
            DuckImpact impact = hit.GetComponentInParent<DuckImpact>();

            if (flight == null || impact == null || impact.HasCrashed)
                continue;

            if (IsImmuneToWind(flight.gameObject))
                continue;

            Vector3 toDuck = flight.transform.position - origin.position;
            float distance = toDuck.magnitude;

            if (distance <= 0.01f)
                continue;

            float angle = Vector3.Angle(origin.forward, toDuck.normalized);
            if (angle > coneAngle)
                continue;

            float t = Mathf.Clamp01(distance / range);
            float strength = windSpeed * falloff.Evaluate(t);

            flight.AddWindVelocity(origin.forward * strength);
        }
    }

    private bool IsImmuneToWind(GameObject duck)
    {
        DuckController controller = duck.GetComponent<DuckController>();
        if (controller != null && controller.Definition != null && controller.Definition.disableFlightControls)
            return true;

        ShieldController shield = duck.GetComponent<ShieldController>();
        if (shield != null && shield.IsActive)
            return true;

        AnvilSlamController anvil = duck.GetComponent<AnvilSlamController>();
        if (anvil != null && anvil.BlocksWind())
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = firePoint != null ? firePoint : transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, range);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin.position, origin.forward * range);
    }
    private void StopShooting()
    {
        _isActive = false;

        if (windParticles != null)
            windParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}