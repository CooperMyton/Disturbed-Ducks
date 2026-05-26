using UnityEngine;
using System;

/// <summary>
/// Attach to any collectible stage objective (blueprint, etc).
/// No health — collected instantly on duck contact.
/// Tag the GameObject with the stage's collectibleTag.
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [Header("Currency Reward")]
    [SerializeField] private int currencyOnCollect = 25;

    [Header("Bobbing")]
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed  = 2f;

    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;

    // StageManager subscribes to this — same pattern as TargetEnemy.OnDied
    public event Action OnCollected;

    private bool    _collected     = false;
    private Vector3 _startPosition;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        if (_collected) return;
        float y = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(_startPosition.x, y, _startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        bool isMainDuck = other.GetComponent<DuckImpact>() != null;
        bool isFamilyDuck = other.GetComponentInParent<DuckFamilyMember>() != null;

        if (!isMainDuck && !isFamilyDuck)
            return;

        Collect();
    }

    private void Collect()
    {
        _collected = true;
        CurrencyManager.Instance?.Add(currencyOnCollect);
        Debug.Log($"{gameObject.name} collected! +{currencyOnCollect} currency");
        OnCollected?.Invoke();
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        Destroy(gameObject);
    }
}