using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DuckFlightController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float maxSpeed = 35f;
    [SerializeField] private float minSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float pitchSpeed = 70f;
    [SerializeField] private float yawSpeed = 70f;
    [SerializeField] private float maxPitchAngle = 60f;

    [Header("Glide Physics")]
    [SerializeField] private float glideGravity = 12f;          // downward pull strength
    [SerializeField] private float maxFallSpeed = 25f;          // terminal velocity
    [SerializeField] private float climbSpeedPenalty = 10f;     // speed lost per second when climbing
    [SerializeField] private float diveSpeedGain = 6f;          // speed gained per second when diving
    [SerializeField] private float noseFollowSpeed = 1.5f;      // how fast nose aligns to glide arc
    [SerializeField] private float pitchDeadzone = 5f;
    [SerializeField] private float dashDecayRate = 8f;          // degrees before climb/dive penalty kicks in

    [Header("Visual Bank")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float maxBankAngle = 25f;
    [SerializeField] private float bankSmoothing = 6f;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private float _currentBankAngle;
    private float _currentSpeed;
    private float _verticalVelocity = 0f;   // gravity accumulates here
    private bool _isLaunched = false;
    private bool _playerIsPitching = false;

    public bool IsLaunched => _isLaunched;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;         // we handle gravity manually
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

        _playerIsPitching = _moveInput.y != 0f;

        ApplyVisualBank(_moveInput.x);
    }

    private void FixedUpdate()
    {
        if (!_isLaunched) return;

        ApplyRotation(_moveInput.y, _moveInput.x);
        ApplyGlidePhysics();
    }

    // -------------------------------------------------------------------------

    public void StartFlight(float launchSpeed, Vector3 initialVector)
    {
        _currentSpeed = Mathf.Min(launchSpeed, maxSpeed);
        _verticalVelocity = 0f;

        // Use _rb.rotation not transform.rotation — sets rotation in physics
        // space immediately, same reason we use rb.position on reset
        _rb.rotation = Quaternion.LookRotation(initialVector, Vector3.up);
        _isLaunched = true;

        GetComponent<DuckController>()?.OnLaunched();
        FlightUIManager.Instance?.OnLaunched();
    }
    public void ApplySpeedPenalty(float penalty)
    {
        _currentSpeed = Mathf.Max(_currentSpeed - penalty, 0f);
        if (_currentSpeed <= minSpeed)
            GetComponent<DuckImpact>()?.Crash();
    }

    public void ApplySpeedBoost(float amount)
    {
        _currentSpeed += amount;
        // Intentionally no clamp here — we allow going over max, decay handles it
    }

    public void SetMaxSpeed(float newMaxSpeed) => maxSpeed = newMaxSpeed;

    public void PrepareForLaunch()
    {
        _isLaunched = false;
        _moveInput = Vector2.zero;
        _currentSpeed = 0f;
        _verticalVelocity = 0f;
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

    private void ApplyGlidePhysics()
    {
        // --- Get current pitch (-180 to 180, negative = nose up) ---
        float pitch = transform.eulerAngles.x > 180f
            ? transform.eulerAngles.x - 360f
            : transform.eulerAngles.x;

        // --- Speed influenced by pitch angle ---
        if (pitch < -pitchDeadzone)
        {
            // Climbing — bleed speed proportional to how steeply we're climbing
            float climbFactor = Mathf.Abs(pitch) / maxPitchAngle;
            _currentSpeed -= climbSpeedPenalty * climbFactor * Time.fixedDeltaTime;
        }
        else if (pitch > pitchDeadzone)
        {
            // Diving — gain speed proportional to dive angle
            float diveFactor = pitch / maxPitchAngle;
            _currentSpeed += diveSpeedGain * diveFactor * Time.fixedDeltaTime;
        }

        if (_currentSpeed > maxSpeed)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, maxSpeed, dashDecayRate * Time.fixedDeltaTime);
        }
        else
        {
            _currentSpeed = Mathf.Clamp(_currentSpeed, minSpeed, maxSpeed);
        }

        // --- Accumulate gravity into vertical velocity ---
        _verticalVelocity -= glideGravity * Time.fixedDeltaTime;
        _verticalVelocity = Mathf.Max(_verticalVelocity, -maxFallSpeed);

        // --- Final velocity: forward thrust + gravity component ---
        Vector3 velocity = transform.forward * _currentSpeed + Vector3.up * _verticalVelocity;
        _rb.linearVelocity = velocity;

        // --- Nose follow: gradually align to actual velocity arc ---
        // Only when player isn't actively pitching — lets gravity
        // naturally pull the nose down during a glide
        if (!_playerIsPitching && velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            // Only blend the pitch axis — player still owns yaw
            Vector3 currentEuler = transform.eulerAngles;
            Vector3 targetEuler  = targetRotation.eulerAngles;

            float blendedPitch = Mathf.LerpAngle(currentEuler.x, targetEuler.x,
                noseFollowSpeed * Time.fixedDeltaTime);

            transform.eulerAngles = new Vector3(blendedPitch, currentEuler.y, 0f);
        }
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

    public void SetBaseStats(float maxSpeed, float turnSpeed, float gravity, float minSpeed)
    {
        this.maxSpeed      = maxSpeed;
        this.pitchSpeed    = turnSpeed;
        this.yawSpeed      = turnSpeed;  // keeping pitch and yaw in sync for now
        this.glideGravity  = gravity;
        this.minSpeed      = minSpeed;
    }

    public void SetManoeuvrability(float turnSpeed)
    {
        pitchSpeed = turnSpeed;
        yawSpeed   = turnSpeed;
    }
}