using UnityEngine;
using UnityEngine.InputSystem;

public class DuckSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject duckRoot;
    [SerializeField] private DuckImpact duckImpact;
    [SerializeField] private LauncherController launcherController; // replaces manual spawn point

    [Header("Reset Key")]
    [SerializeField] private Key resetKey = Key.R;

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (Keyboard.current[resetKey].wasPressedThisFrame)
            ResetDuck();
    }

    public void ResetDuck()
    {
        Rigidbody rb = duckRoot.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Launcher owns the position — let it handle the reset
        launcherController.ResetToLauncher();

        // Re-enable flight state after launcher resets position
        duckImpact.Reset();

        FlightUIManager.Instance?.ShowLaunchPrompt();

        Debug.Log("Duck reset to launcher.");
    }
}