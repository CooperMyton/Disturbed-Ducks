using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DuckFlightController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float minSpeed = 5f; // slowdown floor

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
        _moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            _moveInput.y = 1f;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            _moveInput.y = -1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            _moveInput.x = 1f;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            _moveInput.x = -1f;

        // Launch on first input
        if (!_isLaunched && _moveInput != Vector2.zero)
            Launch();

        ApplyVisualBank(_moveInput.x);
    }

    private void FixedUpdate()
    {
        if (!_isLaunched) return;

        ApplyRotation(_moveInput.y, _moveInput.x);
        ApplyVelocity();
    }

    // -------------------------------------------------------------------------

    private void Launch()
    {
        _isLaunched = true;
        _currentSpeed = forwardSpeed;
        FlightUIManager.Instance?.OnLaunched();
    }

    /// <summary>
    /// Called by Destructible when a box is destroyed.
    /// Reduces speed — if it drops below minSpeed the duck crashes.
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
        _currentSpeed = forwardSpeed;
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
}