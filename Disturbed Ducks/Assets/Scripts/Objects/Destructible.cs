using UnityEngine;

public class Destructible : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip destroyedSound;
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


    [Header("Currency Reward")]
    [SerializeField] private int currencyOnBreak = 10;

    [Header("Explosion Only")]
    [Tooltip("If true, this object can only be damaged by explosions — duck impact does nothing")]
    [SerializeField] private bool explosionOnly = false;

    private float _lastDamageTime = -999f;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        currentHP = maxHP;
        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Explosion-only walls ignore all physical impact
        if (explosionOnly) return;

        if (Time.time - _lastDamageTime < damageCooldown) return;

        Rigidbody hitRb = collision.rigidbody;
        if (hitRb == null) return;

        float impactSpeed = hitRb.linearVelocity.magnitude;
        if (impactSpeed < minSpeedToTakeDamage) return;

        float shieldMultiplier = hitRb.GetComponent<ShieldController>()?.DamageMultiplier ?? 1f;
        float damage = impactSpeed * hitRb.mass * damageMultiplier * shieldMultiplier;

        _lastDamageTime = Time.time;

        TakeDamage(damage, false, hitRb);
        Debug.Log($"{gameObject.name} hit for {damage:F1} | HP: {currentHP:F1}/{maxHP}");
    }

    // -------------------------------------------------------------------------

    /// <param name="amount">Damage to apply.</param>
    /// <param name="fromExplosion">True when called by ExplosionHelper.</param>
    /// <param name="attacker">Rigidbody of the attacker, if any.</param>
    public void TakeDamage(float amount, bool fromExplosion = false, Rigidbody attacker = null)
    {
        if (explosionOnly && !fromExplosion) return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0f);

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);
        if (currentHP <= 0f)
            Break(attacker);
    }

    private void Break(Rigidbody attacker)
    {
        if (attacker != null)
        {
            DuckFlightController duck = attacker.GetComponent<DuckFlightController>();
            duck?.ApplySpeedPenalty(speedPenaltyOnBreak);
        }

        CurrencyManager.Instance?.Add(currencyOnBreak);

        Debug.Log($"{gameObject.name} destroyed! +{currencyOnBreak} currency");
        Destroy(gameObject);
        if (destroyedSound != null)
            AudioSource.PlayClipAtPoint(destroyedSound, transform.position);
    }
}