using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnvilSlamController : MonoBehaviour
{
    [Header("Hover Drift")]
    [SerializeField] private float driftSpeed = 3f;

    [Header("Slam")]
    [SerializeField] private float slamMassMultiplier = 4f;

    [Header("Slam Timeout")]
    [Tooltip("Force a crash if nothing is hit within this many seconds after slamming")]
    [SerializeField] private float slamTimeout = 1.5f;

    private Rigidbody            _rb;
    private DuckFlightController _flightController;
    private DuckImpact           _duckImpact;
    private bool                 _isHovering = false;
    private Vector3 _hoverForward;
    private Vector3 _hoverRight;
    public bool IsSlamActive { get; private set; }

    private void Awake()
    {
        _rb               = GetComponent<Rigidbody>();
        _flightController = GetComponent<DuckFlightController>();
        _duckImpact       = GetComponent<DuckImpact>();
    }

    private void Update()
    {
        if (!_isHovering) return;

        Vector3 drift = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            drift += _hoverForward;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            drift -= _hoverForward;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            drift += _hoverRight;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            drift -= _hoverRight;

        _rb.linearVelocity = drift.normalized * driftSpeed;
    }

    public void StartSlam(float hangTime, float slamForce)
    {
        StartCoroutine(SlamRoutine(hangTime, slamForce));
    }

    private IEnumerator SlamRoutine(float hangTime, float slamForce)
    {
        _hoverForward = transform.forward;
        _hoverForward.y = 0f;
        if (_hoverForward.sqrMagnitude < 0.01f)
            _hoverForward = Vector3.forward;
        _hoverForward.Normalize();

        _hoverRight = transform.right;
        _hoverRight.y = 0f;
        if (_hoverRight.sqrMagnitude < 0.01f)
            _hoverRight = Vector3.right;
        _hoverRight.Normalize();
        // --- Freeze in place ---
        _flightController.enabled = false;
        _rb.linearVelocity        = Vector3.zero;
        _rb.useGravity            = false;
        _isHovering               = true;
        IsSlamActive = true;
        AbilityUI.Instance?.OnBombArmed(hangTime);

        yield return new WaitForSeconds(hangTime);

        _isHovering = false;

        if (_duckImpact.HasCrashed) yield break;

        // --- Boost mass so impact damage scales properly ---
        float originalMass = _rb.mass;
        _rb.mass           = originalMass * slamMassMultiplier;

        // --- Slam: useGravity stays false for consistent impact speed ---
        _rb.useGravity     = false;
        _rb.linearVelocity = Vector3.down * slamForce;

        // --- Wait for impact or timeout ---
        float timer = 0f;
        while (timer < slamTimeout && !_duckImpact.HasCrashed)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset mass here — covers both natural crash and timeout
        _rb.mass = originalMass;

        IsSlamActive = false;

        if (!_duckImpact.HasCrashed)
            _duckImpact.Crash();
    }

    public bool BlocksWind()
    {
        return IsSlamActive || _isHovering;
    }
}