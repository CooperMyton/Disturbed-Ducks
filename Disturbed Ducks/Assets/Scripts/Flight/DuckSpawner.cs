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

    // True while duck is in the air — prevents mid-flight definition swaps
    private bool _inFlight = false;

    // -------------------------------------------------------------------------

    private void Start()
    {
        if (PlayerDuckInventory.Instance != null)
            PlayerDuckInventory.Instance.OnSelectedTypeChanged += OnSelectedTypeChanged;
    }

    private void OnDestroy()
    {
        if (PlayerDuckInventory.Instance != null)
            PlayerDuckInventory.Instance.OnSelectedTypeChanged -= OnSelectedTypeChanged;
    }

    // When the player picks a different duck from the loadout UI while the duck
    // is still on the launcher, immediately swap the definition so the correct
    // duck launches.
    private void OnSelectedTypeChanged(DuckDefinition selected)
    {
        if (_inFlight) return;
        if (selected == null) return;
        duckController.ApplyDefinitionFromType(selected);
        UpgradeManager.Instance?.ApplyCurrentStats();
    }

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (Keyboard.current[nextDuckKey].wasPressedThisFrame)
            TryNextDuck();
    }

    public void TryNextDuck()
    {
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
        _inFlight = false;

        Rigidbody rb = duckRoot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        DuckDefinition selected = PlayerDuckInventory.Instance?.SelectedType;
        if (selected != null)
            duckController.ApplyDefinitionFromType(selected);

        UpgradeManager.Instance?.ApplyCurrentStats();

        launcherController.ResetToLauncher();
        duckImpact.Reset();
        FlightUIManager.Instance?.ShowLaunchPrompt();

        Debug.Log("Duck reset to launcher.");
    }

    // Call this from wherever launch is triggered (LauncherController or DuckController)
    // so DuckSpawner knows not to swap definitions mid-flight.
    public void OnDuckLaunched()
    {
        _inFlight = true;
    }

    /// Called by EndOfAttemptUI restart button
    public void RestartAttempt()
    {
        PlayerDuckInventory.Instance?.ResetRemainingCounts();
        StageManager.RestartCurrentStage();
        ResetDuck();
        LoadoutUI.Instance?.RebuildAndShow();
    }
}