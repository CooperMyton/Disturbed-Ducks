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
        {
            _launchPosition.z += step;
            // check if in line with the launcher, don't want to launch backwards
            if (_launchPosition.z > launchDirectionTarget.position.z)
            {
                _launchPosition.z = launchDirectionTarget.position.z;
            }
        }
        else if (Keyboard.current.xKey.isPressed)
            _launchPosition.z -= step;

        // clamp aim position based on distance from center. upgradeable
        Vector3 offset = _launchPosition - launchDirectionTarget.position;
        Vector3 limitedPosition = Vector3.ClampMagnitude(offset,maxDrawDistance);
        _launchPosition = launchDirectionTarget.position + limitedPosition;
    }

    private void MoveSlingshotString()
    {
        // keep the two ends fixed, and update the middle based on the position of the bird
        if(leftBarPosition && rightBarPosition)
        {
            slingshotString.SetPosition(0,leftBarPosition.position);
            //move the slingshot position slightly behind the bird position while launching
            Vector3 slingshotBackLeft = new Vector3(_launchPosition.x - 0.25f, _launchPosition.y, _launchPosition.z - .6f);
            Vector3 slingshotBackright = new Vector3(_launchPosition.x + 0.25f, _launchPosition.y, _launchPosition.z - .6f);
            slingshotString.SetPosition(1,slingshotBackLeft);
            slingshotString.SetPosition(2,slingshotBackright);
            slingshotString.SetPosition(3,rightBarPosition.position);
        }
    }

    private void LaunchDuck()
    {
        // Use launchDirectionTarget if assigned, otherwise fall back to transform.forward
        
        float launchPower;
        Vector3 direction;
        if (launchDirectionTarget == null)
        {
            Debug.LogWarning("Launch target is null! This should not happen!");
            // default to forwards
            launchPower = 1f;
            direction = transform.forward;
        }
        else
        {
                
            Vector3 offset = launchDirectionTarget.position - _launchPosition;
            launchPower = offset.magnitude;
            direction = offset.normalized;
        }


        Debug.Log("Launched with speed: " + launchSpeed * launchPower);
        Debug.Log("Launch direction: "+ direction.x + ", "+ direction.y + ", " + direction.z);
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
        }
        else
        {
            duckToLaunch.transform.position = _originalLaunchPosition;
        }
    }
}