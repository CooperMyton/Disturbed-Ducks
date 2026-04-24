using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class DuckFlightController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 15f;

    [Header("Rotation")]
    [SerializeField] private float pitchSpeed = 90f;
    [SerializeField] private float yawSpeed = 90f;
    [SerializeField] private float maxPitchAngle = 75f;

    [Header("Visual Bank (cosmetic only)")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float maxBankAngle = 30f;
    [SerializeField] private float bankSmoothing = 6f;

    private Rigidbody _rb;
    private float _currentBankAngle;
    private Vector2 _moveInput;

    private bool isLaunched = false;

    private void Awake()
    {
        
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        // only start flying when the launch is completed.
        // This could be handled by disabling the script instead, but this method arguably allows for more control
        // will need to figure out how this works with mutliple ducks, i.e. how we reset to an "unlaunched" state.
        // work for the future.
        if(!isLaunched)
            return;

        // Read input every frame using new Input System
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
        if(!isLaunched)
            return;
        ApplyRotation(_moveInput.y, _moveInput.x);
        ApplyVelocity();
    }

    private void ApplyRotation(float pitchInput, float yawInput)
    {
        transform.Rotate(Vector3.up, yawInput * yawSpeed * Time.fixedDeltaTime, Space.World);
        transform.Rotate(Vector3.right, -pitchInput * pitchSpeed * Time.fixedDeltaTime, Space.Self);
        ClampPitch();
    }

    private void ApplyVelocity()
    {
        _rb.linearVelocity = transform.forward * forwardSpeed;
    }

    private void ClampPitch()
    {
        Vector3 euler = transform.eulerAngles;
        float pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
        transform.eulerAngles = new Vector3(pitch, euler.y, euler.z);
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
    public void startFlight(float launchSpeed, Vector3 initalVector)
    {
        forwardSpeed = launchSpeed;
        // Note: applying an offset quaternion for now to resolve the issue of the look vector being incorrect
        // this is a hack, would be better to have a non-rotating root and a rotating child, but we can decide what to do about this later.
        Vector3 correctedInitialVector = Quaternion.Euler(0f,0f,90f) * initalVector;
        transform.rotation = Quaternion.LookRotation(correctedInitialVector, Vector3.up);


        isLaunched = true;
    }
}