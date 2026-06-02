using UnityEngine;
using System;

/// <summary>
/// Attach to any stage objective enemy (beaver, etc).
/// Has HP, currency reward, and notifies StageManager on death.
/// Tag the GameObject with the stage's objectiveTag.
/// </summary>
public class TargetEnemy : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 150f;

    [SerializeField] private SimpleWorldHealthBar healthBar;
    [SerializeField] private float currentHP;

    [Header("Currency Reward")]
    [SerializeField] private int currencyOnKill = 50;

    [Header("Impact Settings")]
    [SerializeField] private float minSpeedToTakeDamage = 3f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float damageCooldown = 0.1f;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color healthyColor = Color.yellow;
    [SerializeField] private Color damagedColor = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip passiveSound;
    [SerializeField] private float passiveInterval = 5f;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private float _nextPassiveTime;
    // StageManager subscribes to this
    public event Action OnDied;

    private float _lastDamageTime = -999f;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        currentHP = maxHP;
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();
        healthBar?.SetValue(1f);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;

        Rigidbody hitRb = collision.rigidbody;
        if (hitRb == null) return;

        float speed = hitRb.linearVelocity.magnitude;
        if (speed < minSpeedToTakeDamage) return;

        float shieldMultiplier = hitRb.GetComponent<ShieldController>()?.DamageMultiplier ?? 1f;
        float damage = speed * hitRb.mass * damageMultiplier * shieldMultiplier;

        _lastDamageTime = Time.time;
        TakeDamage(damage);
    }

    public event Action OnDamaged;

    public void TakeDamage(float amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0f);
        healthBar?.SetValue(currentHP / maxHP);
        OnDamaged?.Invoke();
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);
        Debug.Log($"{gameObject.name} took {amount:F1} damage | HP: {currentHP:F1}/{maxHP}");

        if (currentHP <= 0f) Die();
    }

    private void Die()
    {
        CurrencyManager.Instance?.Add(currencyOnKill);
        OnDied?.Invoke();
        Debug.Log($"{gameObject.name} killed! +{currencyOnKill} currency");
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (passiveSound == null || audioSource == null) return;
        if (Time.time < _nextPassiveTime) return;

        audioSource.PlayOneShot(passiveSound);
        _nextPassiveTime = Time.time + passiveInterval;
    }
}