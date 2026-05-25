using UnityEngine;
using UnityEngine.InputSystem;

public class DuckSpawner : MonoBehaviour
{
    public static DuckSpawner Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject duckRoot;
    [SerializeField] private DuckImpact duckImpact;
    [SerializeField] private DuckController duckController;
    [SerializeField] private LauncherController launcherController;
    [SerializeField] private CameraTarget cameraTarget;


    [Header("Keys")]
    [SerializeField] private Key nextDuckKey = Key.R;

    private bool _inFlight = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

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
        // Block R while duck is on the launcher or in flight —
        // only allow when it has actually crashed.
        if (!duckImpact.HasCrashed) return;

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


        ClearProjectiles();


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
        cameraTarget?.SnapToDuck();
        duckImpact.Reset();
        FlightUIManager.Instance?.ShowLaunchPrompt();

        Debug.Log("Duck reset to launcher.");
    }

    public void OnDuckLaunched()
    {
        _inFlight = true;
    }

    public void RestartAttempt()
    {
        PlayerDuckInventory.Instance?.ResetRemainingCounts();
        StageManager.RestartCurrentStage();
        ResetDuck();
        LoadoutUI.Instance?.RebuildAndShow();
    }
    private void ClearProjectiles()
    {
        foreach (var projectile in FindObjectsByType<BeaverProjectile>(FindObjectsSortMode.None))
            Destroy(projectile.gameObject);

        foreach (var projectile in FindObjectsByType<DuckProjectile>(FindObjectsSortMode.None))
            Destroy(projectile.gameObject);
    }


}