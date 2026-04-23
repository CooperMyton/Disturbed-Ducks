using UnityEngine;

/// 
/// Attach to any breakable object (boxes, structures, etc).
/// Damage is calculated from physics impulse, which naturally
/// accounts for the attacker's speed and mass combined.
///
public class Destructible : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    [Header("Impact Settings")]
    [SerializeField] private float damageMultiplier = 0.5f;   // tune this to control how hard it is to break
    [SerializeField] private float minImpulseToTakeDamage = 5f; // prevents damage from tiny bumps

    [Header("Visual Feedback")]
    [SerializeField] private Renderer objectRenderer;   // drag the box renderer here
    [SerializeField] private Color healthyColor = Color.white;
    [SerializeField] private Color damagedColor = Color.red;



    private void Awake()
    {
        currentHP = maxHP;

        // Auto-grab renderer if not assigned in Inspector
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        UpdateColor();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Get the rigidbody of whatever hit us
        Rigidbody hitRb = collision.rigidbody;
        if (hitRb == null) return;

        float impactSpeed = hitRb.linearVelocity.magnitude;

        if (impactSpeed < minImpulseToTakeDamage) return;

        // Damage = speed × mass × multiplier, matching your original instinct
        float damage = impactSpeed * hitRb.mass * damageMultiplier;
        TakeDamage(damage);

        Debug.Log($"{gameObject.name} hit for {damage:F1} | HP: {currentHP:F1}/{maxHP}");
    }


    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f); // never go below 0
        UpdateColor();

        if (currentHP <= 0f)
            Break();
    }

    private void Break()
    {
        // TODO: spawn break particles / sound here later
        Debug.Log($"{gameObject.name} destroyed!");
        Destroy(gameObject);
    }

    private void UpdateColor()
    {
        if (objectRenderer == null) return;

        // Lerp from healthy to damaged color based on how much HP is left
        float healthPercent = currentHP / maxHP;
        objectRenderer.material.color = Color.Lerp(damagedColor, healthyColor, healthPercent);
    }
}