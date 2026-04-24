using UnityEngine;
using UnityEngine.InputSystem;

public class LauncherController : MonoBehaviour
{
    [SerializeField] private GameObject duckToLaunch;

    [Tooltip("Empty child GameObject — rotate this in the scene to aim the launch direction")]
    [SerializeField] private Transform launchDirectionTarget;

    [Tooltip("Base launch speed — clamped by duck's max speed upgrade")]
    [SerializeField] private float launchSpeed = 25f;

    [Tooltip("How fast WASD moves the duck during aiming, in units per second")]
    [SerializeField] private float aimSpeed = 5f;

    private DuckFlightController _flightScript;
    private Rigidbody _rb;
    private bool _inFlight = false;
    private Vector3 _originalLaunchPosition;
    private Vector3 _launchPosition;

    // -------------------------------------------------------------------------

    private void Start()
    {
        _flightScript = duckToLaunch.GetComponent<DuckFlightController>();
        _rb = duckToLaunch.GetComponent<Rigidbody>();

        _originalLaunchPosition = duckToLaunch.transform.position;
        _launchPosition = _originalLaunchPosition;
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
        duckToLaunch.transform.position = _launchPosition;
    }

    // -------------------------------------------------------------------------

    private void HandleAiming()
    {
        // Time.deltaTime makes movement framerate-independent
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
    }

    private void LaunchDuck()
    {
        // Use launchDirectionTarget if assigned, otherwise fall back to transform.forward
        Vector3 direction = launchDirectionTarget != null
            ? (launchDirectionTarget.position - _launchPosition).normalized
            : transform.forward;

        _flightScript.StartFlight(launchSpeed, direction);
    }

    public void ResetToLauncher()
    {
        _inFlight = false;
        _launchPosition = _originalLaunchPosition;

        if (_rb != null)
        {
            _rb.position = _originalLaunchPosition;
            _rb.rotation = Quaternion.identity;
        }
        else
        {
            duckToLaunch.transform.position = _originalLaunchPosition;
        }
    }
}