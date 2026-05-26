using UnityEngine;

public class DuckProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;

    private Vector3 _velocity;
    private float _damage;
    private bool _launched = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed, float damage)
    {
        _velocity = direction.normalized * speed;
        _damage = damage;
        _launched = true;
    }

    private void Update()
    {
        if (!_launched) return;

        transform.position += _velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DuckImpact>() != null)
            return;
        var shield = other.GetComponentInParent<ShieldHuskyFrontShield>();
        if (shield != null)
        {
            shield.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        bool hitSomething = false;

        var generator = other.GetComponentInParent<BossGenerator>();
        if (generator != null)
        {
            generator.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        var boss = other.GetComponentInParent<MechaHuskyBoss>();
        if (boss != null)
        {
            boss.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        var enemy = other.GetComponentInParent<TargetEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
            hitSomething = true;
        }

        var destructible = other.GetComponentInParent<Destructible>();
        if (destructible != null)
        {
            destructible.TakeDamage(_damage);
            hitSomething = true;
        }

        if (hitSomething)
            Destroy(gameObject);
    }
}