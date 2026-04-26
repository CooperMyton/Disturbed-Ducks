using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DuckImpact : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float minSpeedToDisable = 5f;

    [Header("Ground")]
    [SerializeField] private string groundTag = "Ground";

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

        // Ground always crashes regardless of speed
        if (collision.gameObject.CompareTag(groundTag))
        {
            Crash();
            return;
        }

        float currentSpeed = _rb.linearVelocity.magnitude;
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
        _rb.freezeRotation = false;

        if (cameraTarget != null)
            cameraTarget.FreezeYaw();

        GetComponent<AbilityController>()?.OnCrashed();
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
    }
}