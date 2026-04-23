using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DuckImpact : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float minSpeedToDisable = 5f; // how fast duck must be going to crash
    [SerializeField] private LayerMask crashLayers;        // set to Default in Inspector

    [Header("References")]
    [SerializeField] private CameraTarget cameraTarget;

    private Rigidbody _rb;
    private DuckFlightController _flightController;
    private bool _hasCrashed = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _flightController = GetComponent<DuckFlightController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasCrashed) return;

        // Check layer if assigned, otherwise hit anything
        if (crashLayers != 0 && (crashLayers & (1 << collision.gameObject.layer)) == 0)
            return;

        // Use the duck's current speed instead of impulse magnitude
        // since we override linearVelocity every FixedUpdate
        float currentSpeed = _rb.linearVelocity.magnitude;

        Debug.Log($"Hit {collision.gameObject.name} at speed {currentSpeed:F1}");

        if (currentSpeed >= minSpeedToDisable)
            Crash();
    }

    private void Crash()
    {
        _hasCrashed = true;

        if (_flightController != null)
            _flightController.enabled = false;

        _rb.useGravity = true;
        _rb.freezeRotation = false;

        if (cameraTarget != null)
            cameraTarget.FreezeYaw();

        Debug.Log("Duck crashed!");
    }

    public void Reset()
    {
        _hasCrashed = false;
        _rb.useGravity = false;
        _rb.freezeRotation = true;

        if (_flightController != null)
            _flightController.enabled = true;

        if (cameraTarget != null)
            cameraTarget.UnfreezeYaw();
    }
}