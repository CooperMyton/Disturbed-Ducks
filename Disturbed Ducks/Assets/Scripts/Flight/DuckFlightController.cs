using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DuckFlightController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float maxSpeed = 35f;    // upgraded by UpgradeManager
    [SerializeField] private float minSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float pitchSpeed = 70f;
    [SerializeField] private float yawSpeed = 70f;
    [SerializeField] private float maxPitchAngle = 60f;

    [Header("Visual Bank")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float maxBankAngle = 25f;
    [SerializeField] private float bankSmoothing = 6f;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private float _currentBankAngle;
    private float _currentSpeed;
    private bool _isLaunched = false;

    public bool IsLaunched => _isLaunched;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _currentSpeed = forwardSpeed;
    }

    private void Update()
    {
        if (!_isLaunched) return;

        _moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            _moveInput.y = 1f;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            _moveInput.y = -1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            _moveInput.x = 1f;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            _moveInput.x = -1f;

        ApplyVisualBank(_moveInput.x);
    }

    private void FixedUpdate()
    {
        if (!_isLaunched) return;

        ApplyRotation(_moveInput.y, _moveInput.x);
        ApplyVelocity();
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by LauncherController when Space is pressed.
    /// Launch speed is clamped to maxSpeed so upgrades have a meaningful cap.
    /// </summary>
    public void StartFlight(float launchSpeed, Vector3 initialVector)
    {
        // Clamp launch speed to the duck's current max speed
        _currentSpeed = Mathf.Min(launchSpeed, maxSpeed);

        transform.rotation = Quaternion.LookRotation(initialVector, Vector3.up);
        _isLaunched = true;

        GetComponent<AbilityController>()?.OnLaunched();

        FlightUIManager.Instance?.OnLaunched();
    }

    /// <summary>
    /// Called by Destructible when a box is destroyed.
    /// </summary>
    public void ApplySpeedPenalty(float penalty)
    {
        _currentSpeed = Mathf.Max(_currentSpeed - penalty, 0f);

        if (_currentSpeed <= minSpeed)
            GetComponent<DuckImpact>()?.Crash();
    }

    public void PrepareForLaunch()
    {
        _isLaunched = false;
        _moveInput = Vector2.zero;
        _currentSpeed = 0f;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    // -------------------------------------------------------------------------

    private void ApplyRotation(float pitchInput, float yawInput)
    {
        transform.Rotate(Vector3.up, yawInput * yawSpeed * Time.fixedDeltaTime, Space.World);
        transform.Rotate(Vector3.right, -pitchInput * pitchSpeed * Time.fixedDeltaTime, Space.Self);
        ClampPitch();
    }

    private void ApplyVelocity()
    {
        _rb.linearVelocity = transform.forward * _currentSpeed;
    }

    private void ClampPitch()
    {
        Vector3 euler = transform.eulerAngles;
        float pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
        transform.eulerAngles = new Vector3(pitch, euler.y, 0f);
    }

    private void ApplyVisualBank(float yawInput)
    {
        if (modelRoot == null) return;
        float targetBank = -yawInput * maxBankAngle;
        _currentBankAngle = Mathf.Lerp(_currentBankAngle, targetBank, bankSmoothing * Time.deltaTime);
        Vector3 localEuler = modelRoot.localEulerAngles;
        localEuler.z = _currentBankAngle;
        modelRoot.localEulerAngles = localEuler;
    }

    /// <summary>
    /// Called by UpgradeManager when max speed is upgraded.
    /// Does not change current flight speed mid-flight.
    /// </summary>
    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
    }

        public void ApplySpeedMultiplier(float multiplier)
    {
        _currentSpeed = Mathf.Clamp(_currentSpeed * multiplier, minSpeed, maxSpeed);
    }
}