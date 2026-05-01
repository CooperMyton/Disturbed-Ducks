using UnityEngine;
using UnityEngine.InputSystem;

public class DuckSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject duckRoot;
    [SerializeField] private DuckImpact duckImpact;
    [SerializeField] private DuckController duckController;
    [SerializeField] private LauncherController launcherController;

    [Header("Keys")]
    [SerializeField] private Key nextDuckKey = Key.R;

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (Keyboard.current[nextDuckKey].wasPressedThisFrame)
            TryNextDuck();
    }

    public void TryNextDuck()
    {
        //Debug.Log($"TryNextDuck — HasAnyRemaining: {PlayerDuckInventory.Instance?.HasAnyRemaining()}, " +
        //        $"TotalRemaining: {PlayerDuckInventory.Instance?.TotalRemaining()}");

        if (PlayerDuckInventory.Instance == null) return;
        PlayerDuckInventory.Instance.UseSelectedDuck();
        if (!PlayerDuckInventory.Instance.HasAnyRemaining())
        {
            Debug.Log("No ducks remaining — showing EndOfAttemptUI");
            EndOfAttemptUI.Instance?.Show();
            return;
        }

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

        // Apply selected duck type to DuckController
        DuckDefinition selected = PlayerDuckInventory.Instance?.SelectedType;
        if (selected != null)
            duckController.ApplyDefinitionFromType(selected);

        UpgradeManager.Instance?.ApplyCurrentStats();    

        launcherController.ResetToLauncher();
        duckImpact.Reset();
        FlightUIManager.Instance?.ShowLaunchPrompt();

        Debug.Log("Duck reset to launcher.");
    }

    /// Called by EndOfAttemptUI restart button
    public void RestartAttempt()
    {
        PlayerDuckInventory.Instance?.ResetRemainingCounts();
        StageManager.RestartCurrentStage();
        ResetDuck(); // ← ApplyDefinitionFromType fires here
        LoadoutUI.Instance?.RebuildAndShow(); // ← now sees the final state
    }
}