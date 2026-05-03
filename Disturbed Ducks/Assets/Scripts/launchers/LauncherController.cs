using UnityEngine;
using UnityEngine.InputSystem;

public class LauncherController : MonoBehaviour
{
    [SerializeField] private GameObject duckToLaunch;

    [Tooltip("Empty child GameObject — rotate this in the scene to aim the launch direction")]
    [SerializeField] private Transform launchDirectionTarget;

    [Tooltip("Base launch speed multiplier — clamped by duck's max speed upgrade")]
    [SerializeField] private float launchSpeed = 5f;

    [Tooltip("Distance the slingshot can be pulled back. related to launchspeed")]
    [SerializeField] private float maxDrawDistance = 5f;

    [Tooltip("How fast WASD moves the duck during aiming, in units per second")]
    [SerializeField] private float aimSpeed = 5f;

    // Serialized reference so we don't GetComponent every launch
    [SerializeField] private DuckSpawner duckSpawner;

    private DuckFlightController _flightScript;
    private Rigidbody _rb;
    private bool _inFlight = false;
    private Vector3 _originalLaunchPosition;
    private Vector3 _launchPosition;

    // parameters for the string linerender
    [SerializeField] private Transform leftBarPosition;
    [SerializeField] private Transform rightBarPosition;
    private LineRenderer slingshotString;

    // -------------------------------------------------------------------------

    private void Start()
    {
        _flightScript = duckToLaunch.GetComponent<DuckFlightController>();
        _rb = duckToLaunch.GetComponent<Rigidbody>();

        _originalLaunchPosition = duckToLaunch.transform.position;
        _launchPosition = _originalLaunchPosition;

        slingshotString = GetComponent<LineRenderer>();
        slingshotString.positionCount = 4;
    }

    private void Update()
    {
        if (_inFlight) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _inFlight = true;
            LaunchDuck();
            return;
        }

        HandleAiming();
        MoveSlingshotString();
        duckToLaunch.transform.position = _launchPosition;
    }

    // -------------------------------------------------------------------------

    private void HandleAiming()
    {
        float step = aimSpeed * Time.deltaTime;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            _launchPosition.y += step;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            _launchPosition.y -= step;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            _launchPosition.x += step;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            _launchPosition.x -= step;

        if (Keyboard.current.zKey.isPressed)
            _launchPosition.z += step;
        else if (Keyboard.current.xKey.isPressed)
            _launchPosition.z -= step;

        // Clamp total draw distance first
        Vector3 offset = _launchPosition - launchDirectionTarget.position;
        Vector3 limitedOffset = Vector3.ClampMagnitude(offset, maxDrawDistance);
        _launchPosition = launchDirectionTarget.position + limitedOffset;

        // Then clamp Z — prevents pulling behind the launcher after magnitude clamp
        if (_launchPosition.z > launchDirectionTarget.position.z)
            _launchPosition.z = launchDirectionTarget.position.z;
    }

    private void MoveSlingshotString()
    {
        if (leftBarPosition && rightBarPosition)
        {
            slingshotString.SetPosition(0, leftBarPosition.position);
            Vector3 slingshotBackLeft  = new Vector3(_launchPosition.x - 0.25f, _launchPosition.y, _launchPosition.z - .6f);
            Vector3 slingshotBackRight = new Vector3(_launchPosition.x + 0.25f, _launchPosition.y, _launchPosition.z - .6f);
            slingshotString.SetPosition(1, slingshotBackLeft);
            slingshotString.SetPosition(2, slingshotBackRight);
            slingshotString.SetPosition(3, rightBarPosition.position);
        }
    }

    private void LaunchDuck()
    {
        // Notify DuckSpawner that the duck is now in flight so pre-launch
        // definition swaps are blocked until the next reset.
        duckSpawner?.OnDuckLaunched();

        if (launchDirectionTarget == null)
        {
            _flightScript.StartFlight(launchSpeed, transform.forward);
            return;
        }

        Vector3 offset = launchDirectionTarget.position - _launchPosition;
        float launchPower = offset.magnitude;

        if (launchPower < 0.1f)
        {
            Debug.LogWarning("Launch position too close to target — using default forward direction.");
            _flightScript.StartFlight(launchSpeed, transform.forward);
            return;
        }

        Vector3 direction = offset.normalized;

        Debug.Log($"Launched with speed: {launchSpeed * launchPower:F1} | Direction: {direction}");
        _flightScript.StartFlight(launchSpeed * launchPower, direction);
    }

    public void ResetToLauncher()
    {
        _inFlight = false;
        _launchPosition = _originalLaunchPosition;

        if (_rb != null)
        {
            _rb.position = _originalLaunchPosition;
            _rb.rotation = Quaternion.identity;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        else
        {
            duckToLaunch.transform.position = _originalLaunchPosition;
        }

        MoveSlingshotString();
    }
}