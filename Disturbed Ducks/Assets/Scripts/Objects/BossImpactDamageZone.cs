using UnityEngine;

public class BossImpactDamageZone : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private BossGenerator generator;
    [SerializeField] private MechaHuskyBoss boss;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float minImpactSpeed = 3f;
    [SerializeField] private bool crashDuckOnHit = true;

    private void Awake()
    {
        if (generator == null)
            generator = GetComponentInParent<BossGenerator>();

        if (boss == null)
            boss = GetComponentInParent<MechaHuskyBoss>();

        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamageFromDuck(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamageFromDuck(other);
    }

    private void TryDamageFromDuck(Collider other)
    {
        var duckImpact = other.GetComponentInParent<DuckImpact>();
        if (duckImpact == null || duckImpact.HasCrashed)
            return;

        var rb = duckImpact.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        float speed = rb.linearVelocity.magnitude;
        if (speed < minImpactSpeed)
            return;

        float shieldMultiplier = duckImpact.GetComponent<ShieldController>()?.DamageMultiplier ?? 1f;
        float damage = speed * rb.mass * damageMultiplier * shieldMultiplier;

        if (generator != null)
            generator.TakeDamage(damage);

        if (boss != null)
            boss.TakeDamage(damage);

        if (crashDuckOnHit)
            duckImpact.Crash();
    }
}