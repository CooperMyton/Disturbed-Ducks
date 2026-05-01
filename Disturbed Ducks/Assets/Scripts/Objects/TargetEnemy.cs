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

    // StageManager subscribes to this
    public event Action OnDied;

    private float _lastDamageTime = -999f;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        currentHP = maxHP;
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();
        UpdateColor();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;

        Rigidbody hitRb = collision.rigidbody;
        if (hitRb == null) return;

        float speed = hitRb.linearVelocity.magnitude;
        if (speed < minSpeedToTakeDamage) return;

        float damage = speed * hitRb.mass * damageMultiplier;
        _lastDamageTime = Time.time;
        TakeDamage(damage);
    }

    public void TakeDamage(float amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0f);
        UpdateColor();

        Debug.Log($"{gameObject.name} took {amount:F1} damage | HP: {currentHP:F1}/{maxHP}");

        if (currentHP <= 0f) Die();
    }

    private void Die()
    {
        CurrencyManager.Instance?.Add(currencyOnKill);
        OnDied?.Invoke();
        Debug.Log($"{gameObject.name} killed! +{currencyOnKill} currency");
        Destroy(gameObject);
    }

    private void UpdateColor()
    {
        if (objectRenderer == null) return;
        float pct = currentHP / maxHP;
        objectRenderer.material.color = Color.Lerp(damagedColor, healthyColor, pct);
    }
}