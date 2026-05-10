using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public static event System.Action<int> OnStageInitialized;
    // Pairs each StageDefinition with its root GameObject in the scene.
    // Element 0 = Stage 1, Element 1 = Stage 2, etc.
    [System.Serializable]
    private struct StageEntry
    {
        public StageDefinition definition;
        public GameObject      environmentRoot;
    }

    [SerializeField] private StageEntry[] stages;
    [SerializeField] private PlayerInventory inventory;

    private int  _currentIndex       = 0;
    private int  _objectivesRemaining = 0;
    private bool _isCleared           = false;

    private int _objectivesTotal = 0;
    public static event System.Action<int, int> OnObjectivesChanged; // remaining, total

    // Convenience
    private StageDefinition CurrentDef  => stages[_currentIndex].definition;
    private GameObject      CurrentRoot => stages[_currentIndex].environmentRoot;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Activate only stage 0 on scene load — all others off
        for (int i = 0; i < stages.Length; i++)
            stages[i].environmentRoot?.SetActive(i == 0);

        _currentIndex = 0;
        InitializeStage();
    }

    // -------------------------------------------------------------------------

    private void InitializeStage()
    {
        _isCleared           = false;
        _objectivesRemaining = 0;

        var def = CurrentDef;
        if (def == null) return;

        // --- TargetEnemy objectives ---
        foreach (var tag in def.objectiveTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (!IsChildOfCurrentRoot(obj.transform)) continue;
                var enemy = obj.GetComponent<TargetEnemy>();
                if (enemy == null) continue;
                _objectivesRemaining++;
                enemy.OnDied += HandleObjectiveComplete;
            }
        }

        // --- Collectible objectives ---
        foreach (var tag in def.collectibleTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (!IsChildOfCurrentRoot(obj.transform)) continue;
                var col = obj.GetComponent<Collectible>();
                if (col == null) continue;
                _objectivesRemaining++;
                col.OnCollected += HandleObjectiveComplete;
            }
        }

        Debug.Log($"Stage {def.stageName} — {_objectivesRemaining} objectives");
        OnStageInitialized?.Invoke(_currentIndex);

        _objectivesTotal = _objectivesRemaining;
        OnObjectivesChanged?.Invoke(_objectivesRemaining, _objectivesTotal);
    }

    private bool IsChildOfCurrentRoot(Transform t)
    {
        if (CurrentRoot == null) return true; // no root set — accept all
        return t.IsChildOf(CurrentRoot.transform);
    }

    private void HandleObjectiveComplete()
    {
        _objectivesRemaining--;
        OnObjectivesChanged?.Invoke(_objectivesRemaining, _objectivesTotal);
        Debug.Log($"Objective complete — {_objectivesRemaining} remaining");
        if (_objectivesRemaining <= 0)
            TriggerStageClear();
    }

    private void TriggerStageClear()
    {
        if (_isCleared) return;
        _isCleared = true;

        bool isFirstClear = !inventory.HasClearedStage(CurrentDef.stageId);
        if (isFirstClear)
        {
            inventory.MarkStageCleared(CurrentDef.stageId);
            CurrencyManager.Instance?.Add(CurrentDef.firstClearBonus);
        }

        // Final stage — show win screen instead of stage clear
        if (CurrentDef.nextStage == null)
        {
            WinScreenUI.Instance?.Show();
            return;
        }

        StageClearUI.Instance?.Show(CurrentDef, isFirstClear);
    }

    // -------------------------------------------------------------------------

    /// Advances to the next stage in the array without reloading the scene.
    /// Loadout (remaining counts) carries over — no inventory reset.
    public void LoadNextStage()
    {
        int nextIndex = _currentIndex + 1;
        if (nextIndex >= stages.Length)
        {
            Debug.Log("No next stage — all stages complete");
            return;
        }

        PlayerDuckInventory.Instance?.UseSelectedDuck();

        CurrentRoot?.SetActive(false);
        _currentIndex = nextIndex;
        CurrentRoot?.SetActive(true);

        InitializeStage();

        // Check BEFORE resetting to launcher — if no ducks remain,
        // show end of attempt without placing a duck in the launcher
        if (PlayerDuckInventory.Instance != null &&
            !PlayerDuckInventory.Instance.HasAnyRemaining())
        {
            FlightUIManager.Instance?.OnCrashed();
            return;
        }

        DuckSpawner.Instance?.ResetDuck();
        FlightUIManager.Instance?.ShowLaunchPrompt();
    }

    /// Reloads the scene — resets all environments and duck counts back to stage 1.
    /// PlayerDuckInventory survives via DontDestroyOnLoad and ResetRemainingCounts
    /// is called by DuckSpawner.RestartAttempt.
    public static void RestartCurrentStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}