using UnityEngine;
using System.Collections;
public class ExplosiveCar : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    [Header("Impact Damage Intake")]
    [SerializeField] private float minSpeedToTakeDamage = 3f;
    [SerializeField] private float impactDamageMultiplier = 1f;
    [SerializeField] private float damageCooldown = 0.1f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 6f;
    [SerializeField] private float explosionDamage = 150f;
    [SerializeField] private ExplosionDefinition explosionVisualDefinition;
    [SerializeField] private float explosionDelay = 1f;

    [SerializeField] private ParticleSystem idleSmoke;
    [SerializeField] private ParticleSystem warningSmoke;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color healthyColor = Color.white;
    [SerializeField] private Color damagedColor = Color.red;

    private bool _exploded = false;
    private float _lastDamageTime = -999f;

    private Coroutine _pendingExplosion;

    private void Awake()
    {
        currentHP = maxHP;

        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();

        UpdateColor();
        idleSmoke?.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_exploded) return;
        if (Time.time - _lastDamageTime < damageCooldown) return;

        Rigidbody hitRb = collision.rigidbody;
        if (hitRb == null) return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minSpeedToTakeDamage) return;

        float shieldMultiplier = hitRb.GetComponent<ShieldController>()?.DamageMultiplier ?? 1f;
        float damage = speed * hitRb.mass * impactDamageMultiplier * shieldMultiplier;

        _lastDamageTime = Time.time;
        TakeDamage(damage, false);
    }

    public void TakeDamage(float amount, bool fromExplosion = false)
    {
        if (_exploded) return;

        if (fromExplosion)
        {
            StartExplosionDelay();
            return;
        }

        currentHP = Mathf.Max(currentHP - amount, 0f);
        UpdateColor();

        if (currentHP <= 0f)
            StartExplosionDelay();
    }

    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;
        idleSmoke?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        warningSmoke?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (explosionVisualDefinition != null)
        {
            ExplosionHelper.Trigger(transform.position, explosionRadius, explosionDamage, explosionVisualDefinition, this);
            HideCarAfterExplosion();

            float destroyDelay = Mathf.Max(0.05f, explosionVisualDefinition.visualDuration + 0.1f);
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            DamageNearbyWithoutVisual();
            Destroy(gameObject);
        }
    }

    private void DamageNearbyWithoutVisual()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            hit.GetComponent<Destructible>()?.TakeDamage(explosionDamage, true);
            hit.GetComponent<TargetEnemy>()?.TakeDamage(explosionDamage);
            hit.GetComponent<ExplosiveCar>()?.TakeDamage(explosionDamage, true);
        }
    }

    private void UpdateColor()
    {
        if (objectRenderer == null) return;

        float pct = currentHP / maxHP;
        objectRenderer.material.color = Color.Lerp(damagedColor, healthyColor, pct);
    }

        private void HideCarAfterExplosion()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }

    private void StartExplosionDelay()
    {
        if (_exploded || _pendingExplosion != null) return;

        if (objectRenderer != null)
            objectRenderer.material.color = damagedColor;

        warningSmoke?.Play();

        _pendingExplosion = StartCoroutine(ExplosionDelayRoutine());
    }

    private IEnumerator ExplosionDelayRoutine()
    {
        yield return new WaitForSeconds(explosionDelay);
        _pendingExplosion = null;
        Explode();
    }
}
