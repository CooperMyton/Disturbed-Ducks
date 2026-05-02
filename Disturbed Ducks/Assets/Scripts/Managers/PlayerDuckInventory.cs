using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Tracks owned ducks and runtime remaining counts.
/// Owned counts are permanent. Remaining counts reset each attempt.
/// </summary>
public class PlayerDuckInventory : MonoBehaviour
{
    public static PlayerDuckInventory Instance { get; private set; }

    [SerializeField] private PlayerInventory inventory;

    [Header("Loadout Settings")]
    [Tooltip("Maximum total ducks the player can own across all types")]
    [SerializeField] private int maxTotalDucks = 10;

    // Runtime remaining per type — reset at start of each attempt
    private Dictionary<DuckDefinition, int> _remaining
        = new Dictionary<DuckDefinition, int>();

    private DuckDefinition _selectedType;

    public event Action OnInventoryChanged;
    public event Action<DuckDefinition> OnSelectedTypeChanged;

    public int MaxTotalDucks => maxTotalDucks;
    public DuckDefinition SelectedType => _selectedType;

    public int TotalOwned
    {
        get
        {
            int total = 0;
            foreach (var count in inventory.ownedDuckCounts) total += count;
            return total;
        }
    }

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResetRemainingCounts();
        if (inventory.ownedDuckTypes.Count > 0)
            SelectType(inventory.ownedDuckTypes[0]);

        // Debug — tells us exactly what the inventory contains on start
        Debug.Log($"PlayerDuckInventory Start — owned types: {inventory.ownedDuckTypes.Count}");
        for (int i = 0; i < inventory.ownedDuckTypes.Count; i++)
            Debug.Log($"  {inventory.ownedDuckTypes[i]?.duckName}: {inventory.ownedDuckCounts[i]} owned");
        Debug.Log($"  Total remaining: {TotalRemaining()}, HasAny: {HasAnyRemaining()}");
    }

    // -------------------------------------------------------------------------

    /// Resets runtime remaining counts to owned counts — call at attempt start
    public void ResetRemainingCounts()
    {
        _remaining.Clear();
        for (int i = 0; i < inventory.ownedDuckTypes.Count; i++)
        {
            Debug.Log($"ResetRemainingCounts — {inventory.ownedDuckTypes[i]?.duckName}: {inventory.ownedDuckCounts[i]}");
            _remaining[inventory.ownedDuckTypes[i]] = inventory.ownedDuckCounts[i];
        }

        // Re-select first available type — _selectedType may be null if
        // all ducks were exhausted before this reset
        if (_selectedType == null || GetRemaining(_selectedType) == 0)
            AutoSelectNextAvailable();

        OnInventoryChanged?.Invoke();
    }

    public int GetRemaining(DuckDefinition def)
        => _remaining.ContainsKey(def) ? _remaining[def] : 0;

    public int GetOwned(DuckDefinition def)
        => inventory.GetOwnedCount(def);

    public bool HasAnyRemaining()
    {
        foreach (var kvp in _remaining)
            if (kvp.Value > 0) return true;
        return false;
    }

    public int TotalRemaining()
    {
        int total = 0;
        foreach (var kvp in _remaining) total += kvp.Value;
        return total;
    }

    /// Decrements remaining count for the selected type
    public void UseSelectedDuck()
    {
        if (_selectedType == null) return;
        if (!_remaining.ContainsKey(_selectedType)) return;

        _remaining[_selectedType] = Mathf.Max(0, _remaining[_selectedType] - 1);
        OnInventoryChanged?.Invoke();

        // Auto-switch if current type is exhausted
        if (_remaining[_selectedType] == 0)
            AutoSelectNextAvailable();
    }

    public void SelectType(DuckDefinition def)
    {
        if (def == null || GetRemaining(def) <= 0) return;
        _selectedType = def;
        OnSelectedTypeChanged?.Invoke(_selectedType);
    }

    // -------------------------------------------------------------------------

    public bool TryBuyDuck(DuckDefinition def)
    {
        if (TotalOwned >= maxTotalDucks)
        {
            Debug.Log("Duck loadout is full");
            return false;
        }

        int cost = def.GetPurchaseCost(GetOwned(def));
        if (!CurrencyManager.Instance.Spend(cost)) return false;

        inventory.AddDuck(def, 1);
        ResetRemainingCounts();
        return true;
    }

    // -------------------------------------------------------------------------

    private void AutoSelectNextAvailable()
    {
        foreach (var def in inventory.ownedDuckTypes)
        {
            if (GetRemaining(def) > 0)
            {
                SelectType(def);
                return;
            }
        }
        _selectedType = null;
    }

    public List<DuckDefinition> GetOwnedTypes()
        => inventory.ownedDuckTypes;
}