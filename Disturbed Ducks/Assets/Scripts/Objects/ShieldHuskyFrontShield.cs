using UnityEngine;

public class ShieldHuskyFrontShield : MonoBehaviour
{
    [Header("World Lock")]
    [SerializeField] private bool lockWorldTransform = true;

    [Header("Shield Health")]
    [SerializeField] private float maxHP = 120f;

    [Header("Impact Damage")]
    [SerializeField] private float duckImpactDamageMultiplier = 1f;
    [SerializeField] private float minDuckSpeedToDamage = 3f;

    [Header("Visual")]
    [SerializeField] private Renderer shieldRenderer;
    [SerializeField] private Color healthyColor = new Color(0.2f, 0.8f, 1f, 0.7f);
    [SerializeField] private Color damagedColor = new Color(1f, 0.2f, 0.2f, 0.7f);

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;

    private Vector3 _lockedPosition;
    private Quaternion _lockedRotation;
    private float _currentHP;
    private bool _broken;

    private void Awake()
    {
        _lockedPosition = transform.position;
        _lockedRotation = transform.rotation;
        _currentHP = maxHP;

        if (shieldRenderer == null)
            shieldRenderer = GetComponentInChildren<Renderer>();

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        UpdateVisual();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleDuckHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleDuckHit(other);
    }

    private void HandleDuckHit(Collider other)
    {
        if (_broken) return;

        var duckImpact = other.GetComponentInParent<DuckImpact>();
        var duckRb = other.GetComponentInParent<Rigidbody>();

        if (duckImpact == null || duckRb == null || duckImpact.HasCrashed)
            return;

        float speed = duckRb.linearVelocity.magnitude;

        if (speed >= minDuckSpeedToDamage)
        {
            float damage = speed * duckRb.mass * duckImpactDamageMultiplier;
            TakeDamage(damage);
        }

        duckImpact.Crash();
    }

    public void TakeDamage(float amount)
    {
        if (_broken) return;

        _currentHP = Mathf.Max(_currentHP - amount, 0f);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        UpdateVisual();

        if (_currentHP <= 0f)
            BreakShield();
    }

    private void BreakShield()
    {
        _broken = true;

        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);

        gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        if (shieldRenderer == null) return;

        float pct = maxHP > 0f ? _currentHP / maxHP : 0f;
        shieldRenderer.material.color = Color.Lerp(damagedColor, healthyColor, pct);
    }

    private void LateUpdate()
    {
        if (!lockWorldTransform || _broken) return;

        transform.position = _lockedPosition;
        transform.rotation = _lockedRotation;
    }
}