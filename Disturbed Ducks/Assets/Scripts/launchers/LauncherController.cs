using UnityEngine;
using UnityEngine.InputSystem;

public class LauncherController : MonoBehaviour
{
    [SerializeField] private GameObject duckToLaunch;

    private DuckFlightController flightScript;

    private bool inFlight = false;
    private Vector3 launchPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flightScript = duckToLaunch.GetComponent<DuckFlightController>();
        launchPosition = duckToLaunch.transform.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(inFlight)
            return;



        if(Keyboard.current.spaceKey.isPressed)
        {
            inFlight = true;
            launchDuck();
        }

        // if we're still in launch mode, update position based on key presses.
        
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            launchPosition.y = launchPosition.y + .1f;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            launchPosition.y = launchPosition.y - .1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            launchPosition.x = launchPosition.x + .1f;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            launchPosition.x = launchPosition.x - .1f;

            
        if (Keyboard.current.zKey.isPressed)
            launchPosition.z = launchPosition.z + .1f;
        else if (Keyboard.current.xKey.isPressed)
            launchPosition.z = launchPosition.z - .1f;

        // NOTE: in the future, instead of having the real flight object move, could replace it with a "dummy" duck prefab
        // then when we transition to flight mode, we would hide the prefab and transition to showing the real player duck


        duckToLaunch.transform.position = launchPosition;
    }

    void launchDuck()
    {
        // take the current launch settings and pass it to the flight controller while telling it to activate
        Vector3 launchVector = transform.position - launchPosition;
        Vector3 normalizedLaunchVector = launchVector.normalized;

        flightScript.startFlight(launchVector.magnitude,launchVector.normalized);
    }
}
