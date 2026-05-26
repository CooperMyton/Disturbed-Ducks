using UnityEngine;

public class MechaHuskyBoss : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHP = 500f;
    [SerializeField] private SimpleWorldHealthBar healthBar;
    [SerializeField] private Color protectedBarColor = Color.magenta;
    [SerializeField] private Color vulnerableBarColor = Color.red;

    [Header("Movement")]
    [SerializeField] private float patrolDistance = 8f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float pauseDuration = 0.8f;
    [SerializeField] private bool patrolAlongLocalRight = true;

    private Vector3 _pointA;
    private Vector3 _pointB;
    private Vector3 _targetPoint;
    private bool _isPausing;
    private float _pauseTimer;

    [Header("Net Attack")]
    [SerializeField] private GameObject netProjectilePrefab;
    [SerializeField] private Transform pawFirePoint;
    [SerializeField] private float netInterval = 3f;
    [SerializeField] private float netSpeed = 20f;

    private float _hp;
    private int _pathIndex;
    private float _netTimer;
    private bool _vulnerable;
    private bool _phaseOneDefeated;

    public bool PhaseOneDefeated => _phaseOneDefeated;
    public event System.Action OnPhaseOneDefeated;

    private void Awake()
    {
        _hp = maxHP;
        _netTimer = netInterval;

        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 patrolDirection = patrolAlongLocalRight ? transform.right : transform.forward;
        patrolDirection.y = 0f;

        if (patrolDirection.sqrMagnitude < 0.01f)
            patrolDirection = Vector3.right;
        else
            patrolDirection.Normalize();

        _pointA = transform.position - patrolDirection * patrolDistance;
        _pointB = transform.position + patrolDirection * patrolDistance;
        _targetPoint = _pointB;

        healthBar?.SetValue(1f);
        healthBar?.SetColor(protectedBarColor);
    }

    private void Update()
    {
        if (_phaseOneDefeated) return;

        MoveAlongPath();
        HandleNetAttack();
    }

    private void MoveAlongPath()
    {
        if (_isPausing)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f)
                _isPausing = false;

            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPoint,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _targetPoint) <= 0.1f)
        {
            _targetPoint = Vector3.Distance(_targetPoint, _pointA) < 0.1f ? _pointB : _pointA;
            _isPausing = true;
            _pauseTimer = pauseDuration;
        }
    }

    private void HandleNetAttack()
    {
        _netTimer -= Time.deltaTime;
        if (_netTimer > 0f) return;

        _netTimer = netInterval;

        if (netProjectilePrefab == null || pawFirePoint == null) return;

        GameObject net = Instantiate(netProjectilePrefab, pawFirePoint.position, pawFirePoint.rotation);
        var projectile = net.GetComponent<SecurityNetProjectile>();
        if (projectile != null)
            projectile.Launch(pawFirePoint.forward, netSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var duck = collision.rigidbody?.GetComponent<DuckImpact>();
        if (duck == null)
            duck = collision.collider.GetComponentInParent<DuckImpact>();

        var rb = collision.rigidbody;
        if (rb == null && duck != null)
            rb = duck.GetComponent<Rigidbody>();

        if (duck == null || rb == null || duck.HasCrashed) return;

        float damage = rb.linearVelocity.magnitude * rb.mass;
        TakeDamage(damage);
        duck.Crash();
    }

    public void SetVulnerable(bool vulnerable)
    {
        _vulnerable = vulnerable;
        healthBar?.SetColor(_vulnerable ? vulnerableBarColor : protectedBarColor);
    }

    public void TakeDamage(float amount)
    {
        if (_phaseOneDefeated) return;
        if (!_vulnerable) return;

        _hp = Mathf.Max(_hp - amount, 0f);
        healthBar?.SetValue(_hp / maxHP);

        if (_hp <= 0f)
        {
            _phaseOneDefeated = true;
            OnPhaseOneDefeated?.Invoke();
        }
    }
}