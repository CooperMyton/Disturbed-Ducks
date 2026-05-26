using UnityEngine;

public class BossGenerator : MonoBehaviour
{
    [SerializeField] private float maxHP = 150f;
    [SerializeField] private SimpleWorldHealthBar healthBar;

    private float _hp;
    private bool _destroyed;

    public bool IsDestroyed => _destroyed;
    public event System.Action<BossGenerator> OnDestroyed;

    private void Awake()
    {
        _hp = maxHP;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        healthBar?.SetValue(1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_destroyed) return;

        var duck = collision.rigidbody?.GetComponent<DuckImpact>();
        if (duck == null)
            duck = collision.collider.GetComponentInParent<DuckImpact>();

        var rb = collision.rigidbody;
        if (rb == null && duck != null)
            rb = duck.GetComponent<Rigidbody>();

        if (duck == null || rb == null) return;

        float damage = rb.linearVelocity.magnitude * rb.mass;
        TakeDamage(damage);
        duck.Crash();
    }

    public void TakeDamage(float amount)
    {
        if (_destroyed) return;

        _hp = Mathf.Max(_hp - amount, 0f);
        healthBar?.SetValue(_hp / maxHP);

        if (_hp <= 0f)
        {
            _destroyed = true;
            OnDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}