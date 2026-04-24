using UnityEngine;

public class Destructible : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    [Header("Impact Settings")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float minSpeedToTakeDamage = 3f;
    [SerializeField] private float damageCooldown = 0.1f;

    [Header("On Destroy")]
    [Tooltip("How much speed the duck loses when this object is destroyed")]
    [SerializeField] private float speedPenaltyOnBreak = 6f;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color healthyColor = Color.white;
    [SerializeField] private Color damagedColor = Color.red;

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

        float impactSpeed = hitRb.linearVelocity.magnitude;
        if (impactSpeed < minSpeedToTakeDamage) return;

        float damage = impactSpeed * hitRb.mass * damageMultiplier;
        _lastDamageTime = Time.time;

        TakeDamage(damage, hitRb);
        Debug.Log($"{gameObject.name} hit for {damage:F1} | HP: {currentHP:F1}/{maxHP}");
    }

    // -------------------------------------------------------------------------

    public void TakeDamage(float amount, Rigidbody attacker = null)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f);
        UpdateColor();

        if (currentHP <= 0f)
            Break(attacker);
    }

    private void Break(Rigidbody attacker)
    {
        // Notify the duck so it loses speed
        if (attacker != null)
        {
            DuckFlightController duck = attacker.GetComponent<DuckFlightController>();
            duck?.ApplySpeedPenalty(speedPenaltyOnBreak);
        }

        Debug.Log($"{gameObject.name} destroyed!");
        Destroy(gameObject);
    }

    private void UpdateColor()
    {
        if (objectRenderer == null) return;
        float healthPercent = currentHP / maxHP;
        objectRenderer.material.color = Color.Lerp(damagedColor, healthyColor, healthPercent);
    }
}