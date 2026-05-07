using System.Collections;
using UnityEngine;

public class AnvilSlamController : MonoBehaviour
{
    private Rigidbody            _rb;
    private DuckFlightController _flightController;
    private DuckImpact           _duckImpact;

    private void Awake()
    {
        _rb               = GetComponent<Rigidbody>();
        _flightController = GetComponent<DuckFlightController>();
        _duckImpact       = GetComponent<DuckImpact>();
    }

    public void StartSlam(float hangTime, float slamForce)
    {
        StartCoroutine(SlamRoutine(hangTime, slamForce));
    }

    private IEnumerator SlamRoutine(float hangTime, float slamForce)
    {
        // Freeze in place
        _flightController.enabled = false;
        _rb.linearVelocity        = Vector3.zero;
        _rb.useGravity            = false;

        yield return new WaitForSeconds(hangTime);

        // Safety — if something crashed us during the hang, don't slam
        if (_duckImpact.HasCrashed) yield break;

        // Slam straight down
        _rb.useGravity     = true;
        _rb.linearVelocity = Vector3.down * slamForce;
    }
}