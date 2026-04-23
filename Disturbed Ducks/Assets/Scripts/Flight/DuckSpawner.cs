using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Owns the duck's spawn point and handles resetting it.
/// To change the spawn point later: create an empty GameObject where
/// you want the launch position and drag it into the Spawn Point field.
/// </summary>
public class DuckSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject duckRoot;
    [SerializeField] private DuckImpact duckImpact;
    [SerializeField] private DuckFlightController duckFlightController;

    [Header("Spawn Point")]
    [Tooltip("Leave empty to use the duck's starting position. " +
             "Assign an empty GameObject here to set a custom spawn point later.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Reset Key")]
    [SerializeField] private Key resetKey = Key.R;

    private Vector3 _defaultSpawnPosition;
    private Quaternion _defaultSpawnRotation;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _defaultSpawnPosition = duckRoot.transform.position;
        _defaultSpawnRotation = duckRoot.transform.rotation;
    }

    private void Update()
    {
        if (Keyboard.current[resetKey].wasPressedThisFrame)
            ResetDuck();
    }

    // -------------------------------------------------------------------------

    public void ResetDuck()
    {
        Vector3 targetPosition = spawnPoint != null
            ? spawnPoint.position
            : _defaultSpawnPosition;

        Quaternion targetRotation = spawnPoint != null
            ? spawnPoint.rotation
            : _defaultSpawnRotation;

        Rigidbody rb = duckRoot.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // 1. Zero velocity FIRST — clear all crash momentum before anything else
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 2. Move via rb.position not transform — teleports in physics space immediately
            //    transform.SetPositionAndRotation queues a reconciliation that takes
            //    a few frames to resolve, causing the delay you were seeing
            rb.position = targetPosition;
            rb.rotation = targetRotation;
        }

        // 3. Reset flight state LAST — flight controller starts clean with no leftover velocity
        duckImpact.Reset();

        Debug.Log("Duck reset to spawn point.");
    }
}