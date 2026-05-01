using UnityEngine;
using System;

/// <summary>
/// Owns the player's currency balance.
/// All spending and earning goes through here.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private PlayerInventory inventory;

    // Fired whenever balance changes — UI subscribes to this
    public event Action<int> OnBalanceChanged;

    public int Balance => inventory.currency;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        OnBalanceChanged?.Invoke(Balance);
    }

    // -------------------------------------------------------------------------

    public void Add(int amount)
    {
        inventory.currency += amount;
        OnBalanceChanged?.Invoke(Balance);
        Debug.Log($"Currency +{amount} | Total: {Balance}");
    }

    public bool CanAfford(int cost) => Balance >= cost;

    public bool Spend(int amount)
    {
        if (!CanAfford(amount)) return false;
        inventory.currency -= amount;
        OnBalanceChanged?.Invoke(Balance);
        Debug.Log($"Currency -{amount} | Total: {Balance}");
        return true;
    }
}