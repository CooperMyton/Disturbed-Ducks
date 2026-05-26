using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DuckImpact : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float minSpeedToDisable = 5f;

    [Header("Ground")]
    [SerializeField] private string groundTag = "Ground";

    [Header("Obstacle")]
    [SerializeField] private string obstacleTag = "obstacle";

    [Header("References")]
    [SerializeField] private CameraTarget cameraTarget;

    private Rigidbody _rb;
    private DuckFlightController _flightController;
    private bool _hasCrashed = false;

    public bool HasCrashed => _hasCrashed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _flightController = GetComponent<DuckFlightController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasCrashed) return;

        float currentSpeed = _rb.linearVelocity.magnitude;

        ApplyFinalBossImpactDamage(collision, currentSpeed);

        if (collision.gameObject.CompareTag(groundTag) ||
            collision.gameObject.CompareTag(obstacleTag) ||
            collision.gameObject.GetComponentInParent<DuckCrashObstacle>() != null)
        {
            Crash();
            return;
        }

        Debug.Log($"Hit {collision.gameObject.name} at speed {currentSpeed:F1}");

        if (currentSpeed >= minSpeedToDisable)
            Crash();
    }

    public void Crash()
    {
        if (_hasCrashed) return;
        _hasCrashed = true;

        if (_flightController != null)
            _flightController.enabled = false;

        _rb.useGravity = true;
        _rb.freezeRotation = true;

        if (cameraTarget != null)
            cameraTarget.FreezeYaw();

        GetComponent<AbilityController>()?.OnCrashed();
        GetComponent<DuckController>()?.OnCrashed();


        FlightUIManager.Instance?.OnCrashed();

        Debug.Log("Duck crashed!");
    }

    public void Reset()
    {
        _hasCrashed = false;
        _rb.useGravity = false;
        _rb.freezeRotation = true;

        if (_flightController != null)
        {
            _flightController.enabled = true;
            _flightController.PrepareForLaunch();
        }

        if (cameraTarget != null)
            cameraTarget.UnfreezeYaw();

        GetComponent<AbilityController>()?.OnReset();

        GetComponent<DuckController>()?.OnReset();
    }
    private void ApplyFinalBossImpactDamage(Collision collision, float impactSpeed)
    {
        if (collision == null) return;

        float shieldMultiplier = GetComponent<ShieldController>()?.DamageMultiplier ?? 1f;
        float damage = impactSpeed * _rb.mass * shieldMultiplier;

        if (damage <= 0f) return;

        var generator = collision.collider.GetComponentInParent<BossGenerator>();
        if (generator != null)
            generator.TakeDamage(damage);

        var boss = collision.collider.GetComponentInParent<MechaHuskyBoss>();
        if (boss != null)
            boss.TakeDamage(damage);
    }
}