using UnityEngine;
using UnityEngine.InputSystem;

/// 
/// Owns the duck's spawn point and handles resetting it.
/// To change the spawn point later: create an empty GameObject where
/// you want the launch position and drag it into the Spawn Point field.
/// 
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

    // Stored on Awake so we always have a fallback
    private Vector3 _defaultSpawnPosition;
    private Quaternion _defaultSpawnRotation;



    private void Awake()
    {
        // Store duck's starting transform as the default spawn
        _defaultSpawnPosition = duckRoot.transform.position;
        _defaultSpawnRotation = duckRoot.transform.rotation;
    }

    private void Update()
    {
        if (Keyboard.current[resetKey].wasPressedThisFrame)
            ResetDuck();
    }



    public void ResetDuck()
    {
        // Use assigned spawn point if one exists, otherwise use original position
        Vector3 targetPosition = spawnPoint != null
            ? spawnPoint.position
            : _defaultSpawnPosition;

        Quaternion targetRotation = spawnPoint != null
            ? spawnPoint.rotation
            : _defaultSpawnRotation;

        // Move the duck
        duckRoot.transform.SetPositionAndRotation(targetPosition, targetRotation);

        // Zero out velocity so it doesn't carry momentum from the crash
        Rigidbody rb = duckRoot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Re-enable flight controls and unfreeze camera
        duckImpact.Reset();

        Debug.Log("Duck reset to spawn point.");
    }
}