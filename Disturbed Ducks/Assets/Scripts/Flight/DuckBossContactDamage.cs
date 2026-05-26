using UnityEngine;

public class DuckBossContactDamage : MonoBehaviour
{
    [SerializeField] private float checkRadius = 0.9f;
    [SerializeField] private float minDamageSpeed = 3f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private LayerMask hitLayers = ~0;

    private Rigidbody _rb;
    private DuckImpact _impact;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _impact = GetComponent<DuckImpact>();
    }

    private void FixedUpdate()
    {
        if (_rb == null || _impact == null || _impact.HasCrashed)
            return;

        float speed = _rb.linearVelocity.magnitude;
        if (speed < minDamageSpeed)
            return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            checkRadius,
            hitLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit.transform.IsChildOf(transform)) continue;

            var generator = hit.GetComponentInParent<BossGenerator>();
            var boss = hit.GetComponentInParent<MechaHuskyBoss>();

            if (generator == null && boss == null)
                continue;

            float shieldMultiplier = GetComponent<ShieldController>()?.DamageMultiplier ?? 1f;
            float damage = speed * _rb.mass * damageMultiplier * shieldMultiplier;

            if (generator != null)
                generator.TakeDamage(damage);

            if (boss != null)
                boss.TakeDamage(damage);

            _impact.Crash();
            return;
        }
    }
}