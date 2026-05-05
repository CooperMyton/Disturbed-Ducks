using UnityEngine;
using System;

/// <summary>
/// Attach to any collectible stage objective (blueprint, etc).
/// No health — collected instantly on duck contact.
/// Tag the GameObject with the stage's collectibleTag.
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Currency Reward")]
    [SerializeField] private int currencyOnCollect = 25;

    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;

    // StageManager subscribes to this — same pattern as TargetEnemy.OnDied
    public event Action OnCollected;

    private bool _collected = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        // Make sure the collider is a trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        // Only the duck should collect it — duck has a Rigidbody
        if (other.GetComponent<Rigidbody>() == null) return;

        Collect();
    }

    private void Collect()
    {
        _collected = true;
        CurrencyManager.Instance?.Add(currencyOnCollect);
        Debug.Log($"{gameObject.name} collected! +{currencyOnCollect} currency");
        OnCollected?.Invoke();
        Destroy(gameObject);
    }
}