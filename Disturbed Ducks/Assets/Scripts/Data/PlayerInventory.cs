using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that persists player progress.
/// Stores currency, owned duck counts, and upgrade levels per duck type.
/// Create one asset: Assets/Data/PlayerInventory.
/// </summary>
[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Game/Player Inventory")]
public class PlayerInventory : ScriptableObject
{
    [Header("Currency")]
    public int currency = 0;

    [Header("Duck Ownership")]
    /// Parallel arrays — Unity can't serialize Dictionary in Inspector
    public List<DuckDefinition> ownedDuckTypes = new List<DuckDefinition>();
    public List<int> ownedDuckCounts = new List<int>();

    [Header("Upgrade Levels — parallel to ownedDuckTypes")]
    public List<int> speedLevels        = new List<int>();
    public List<int> maneurLevels       = new List<int>();
    public List<int> abilityLevels      = new List<int>();

    [Header("Starting State — used by Reset All Progress")]
    [SerializeField] private DuckDefinition startingDuck;
    [SerializeField] private int startingDuckCount = 3;


    [Header("Stage Progress")]
    public List<string> clearedStageIds = new List<string>();

    // -------------------------------------------------------------------------

    public int GetOwnedCount(DuckDefinition def)
    {
        int idx = ownedDuckTypes.IndexOf(def);
        return idx >= 0 ? ownedDuckCounts[idx] : 0;
    }

    public void AddDuck(DuckDefinition def, int count = 1)
    {
        int idx = ownedDuckTypes.IndexOf(def);
        if (idx >= 0)
        {
            ownedDuckCounts[idx] += count;
        }
        else
        {
            ownedDuckTypes.Add(def);
            ownedDuckCounts.Add(count);
            speedLevels.Add(0);
            maneurLevels.Add(0);
            abilityLevels.Add(0);
        }
    }

    public int GetSpeedLevel(DuckDefinition def)    => GetLevel(speedLevels, def);
    public int GetManeurLevel(DuckDefinition def)   => GetLevel(maneurLevels, def);
    public int GetAbilityLevel(DuckDefinition def)  => GetLevel(abilityLevels, def);

    public void SetSpeedLevel(DuckDefinition def, int level)
        => SetLevel(speedLevels, def, level);
    public void SetManeurLevel(DuckDefinition def, int level)
        => SetLevel(maneurLevels, def, level);
    public void SetAbilityLevel(DuckDefinition def, int level)
        => SetLevel(abilityLevels, def, level);

    public bool HasClearedStage(string stageId)
        => clearedStageIds.Contains(stageId);
    public void MarkStageCleared(string stageId)
    {
        if (!clearedStageIds.Contains(stageId))
            clearedStageIds.Add(stageId);
    }

    /// Resets runtime state — does NOT wipe owned ducks or currency
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        currency = 0;
        ownedDuckTypes.Clear();
        ownedDuckCounts.Clear();
        speedLevels.Clear();
        maneurLevels.Clear();
        abilityLevels.Clear();
        clearedStageIds.Clear();

        // Re-add starting duck so player isn't left with nothing
        if (startingDuck != null)
            AddDuck(startingDuck, startingDuckCount);
    }

    // -------------------------------------------------------------------------

    private int GetLevel(List<int> levels, DuckDefinition def)
    {
        int idx = ownedDuckTypes.IndexOf(def);
        return idx >= 0 && idx < levels.Count ? levels[idx] : 0;
    }

    private void SetLevel(List<int> levels, DuckDefinition def, int level)
    {
        int idx = ownedDuckTypes.IndexOf(def);
        if (idx >= 0 && idx < levels.Count)
            levels[idx] = level;
    }
}
