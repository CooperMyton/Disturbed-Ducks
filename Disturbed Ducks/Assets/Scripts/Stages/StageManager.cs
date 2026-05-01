using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private StageDefinition currentStage;
    [SerializeField] private PlayerInventory inventory;

    private List<TargetEnemy> _objectives = new List<TargetEnemy>();
    private int _objectivesRemaining = 0;
    private bool _isCleared = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        InitializeStage();
    }

    // -------------------------------------------------------------------------

    private void InitializeStage()
    {
        _objectives.Clear();
        _isCleared = false;

        GameObject[] tagged = GameObject.FindGameObjectsWithTag(
            currentStage.objectiveTag);

        foreach (var obj in tagged)
        {
            TargetEnemy enemy = obj.GetComponent<TargetEnemy>();
            if (enemy != null)
            {
                _objectives.Add(enemy);
                enemy.OnDied += HandleObjectiveDied;
            }
        }

        _objectivesRemaining = _objectives.Count;
        Debug.Log($"Stage {currentStage.stageName} — {_objectivesRemaining} objectives");
    }

    private void HandleObjectiveDied()
    {
        _objectivesRemaining--;
        if (_objectivesRemaining <= 0)
            TriggerStageClear();
    }

    private void TriggerStageClear()
    {
        if (_isCleared) return;
        _isCleared = true;

        bool isFirstClear = !inventory.HasClearedStage(currentStage.stageId);
        if (isFirstClear)
        {
            inventory.MarkStageCleared(currentStage.stageId);
            CurrencyManager.Instance?.Add(currentStage.firstClearBonus);
        }

        StageClearUI.Instance?.Show(currentStage, isFirstClear);
    }

    public void LoadNextStage()
    {
        if (currentStage.nextStage != null)
        {
            currentStage = currentStage.nextStage;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("No next stage defined");
        }
    }

    /// <summary>
    /// Called by DuckSpawner.RestartAttempt — reloads scene to
    /// respawn all destroyed enemies and destructibles
    /// </summary>
    public static void RestartCurrentStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}