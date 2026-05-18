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

    private Vector3 _launchLocalOffset;

    private Quaternion _launchLocalRotationOffset;
    private Quaternion _originalLaunchRotation;



    // parameters for the string linerender
    [SerializeField] private Transform leftBarPosition;
    [SerializeField] private Transform rightBarPosition;
    private LineRenderer slingshotString;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _flightScript = duckToLaunch.GetComponent<DuckFlightController>();
        _rb = duckToLaunch.GetComponent<Rigidbody>();

        _launchLocalOffset = transform.InverseTransformPoint(duckToLaunch.transform.position);

        _launchLocalRotationOffset = Quaternion.Inverse(transform.rotation) * duckToLaunch.transform.rotation;
        _originalLaunchRotation = duckToLaunch.transform.rotation;

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

        Vector3 localLaunchPosition = transform.InverseTransformPoint(_launchPosition);
        Vector3 localTargetPosition = transform.InverseTransformPoint(launchDirectionTarget.position);

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            localLaunchPosition.y += step;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            localLaunchPosition.y -= step;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            localLaunchPosition.x += step;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            localLaunchPosition.x -= step;

        if (Keyboard.current.zKey.isPressed)
            localLaunchPosition.z += step;
        else if (Keyboard.current.xKey.isPressed)
            localLaunchPosition.z -= step;

        Vector3 localOffset = localLaunchPosition - localTargetPosition;
        Vector3 limitedOffset = Vector3.ClampMagnitude(localOffset, maxDrawDistance);
        localLaunchPosition = localTargetPosition + limitedOffset;

        if (localLaunchPosition.z > localTargetPosition.z)
            localLaunchPosition.z = localTargetPosition.z;

        _launchPosition = transform.TransformPoint(localLaunchPosition);
    }


    private void MoveSlingshotString()
    {
        if (leftBarPosition && rightBarPosition)
        {
            slingshotString.SetPosition(0, leftBarPosition.position);

            Vector3 localLaunchPosition = transform.InverseTransformPoint(_launchPosition);
            Vector3 slingshotBackLeftLocal  = localLaunchPosition + new Vector3(-0.25f, 0f, -0.6f);
            Vector3 slingshotBackRightLocal = localLaunchPosition + new Vector3( 0.25f, 0f, -0.6f);

            slingshotString.SetPosition(1, transform.TransformPoint(slingshotBackLeftLocal));
            slingshotString.SetPosition(2, transform.TransformPoint(slingshotBackRightLocal));
            slingshotString.SetPosition(3, rightBarPosition.position);
        }
    }


    private void LaunchDuck()
    {
        // Notify DuckSpawner that the duck is now in flight so pre-launch
        // definition swaps are blocked until the next reset.
        if (duckSpawner == null)
            duckSpawner = DuckSpawner.Instance;

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
            _rb.rotation = _originalLaunchRotation;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        else
        {
            duckToLaunch.transform.SetPositionAndRotation(_originalLaunchPosition, _originalLaunchRotation);

        }

        if (launchDirectionTarget != null)
        {
            Vector3 launchForward = transform.forward;
            Vector3 duckForward = duckToLaunch.transform.forward;
            float alignment = Vector3.Dot(launchForward.normalized, duckForward.normalized);

            if (alignment < 0.75f)
                Debug.LogWarning($"Duck and launcher directions are misaligned. Dot: {alignment:F2}");
        }


        MoveSlingshotString();
    }
    public void MoveToStageLaunchPoint(Transform launchPoint)
    {
        if (launchPoint == null)
        {
            Debug.LogError("LauncherController: launchPoint was null.");
            return;
        }

        transform.SetPositionAndRotation(launchPoint.position, launchPoint.rotation);


        _originalLaunchPosition = transform.TransformPoint(_launchLocalOffset);
        _launchPosition = _originalLaunchPosition;

        _originalLaunchRotation = transform.rotation * _launchLocalRotationOffset;


        Debug.Log($"Launcher moved to {transform.position}; duck reset point is {_originalLaunchPosition}");

        ResetToLauncher();
    }

}