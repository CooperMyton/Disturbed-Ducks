using UnityEngine;

/// Rotates to face the duck and fires projectiles after launch.
/// Add alongside TargetEnemy — does not replace it.
public class ShootingBeaver : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate    = 1.5f;
    [SerializeField] private float bulletSpeed = 20f;

    [Header("References")]
    [SerializeField] private Transform firePoint;

    private float _fireTimer  = 0f;
    private bool  _isShooting = true;

    private Transform            _duck;
    private DuckFlightController _duckFlight;
    private DuckImpact           _duckImpact;

    private void Start()
    {
        var duckRoot = GameObject.FindGameObjectWithTag("Duck");
        if (duckRoot != null)
        {
            _duck       = duckRoot.transform;
            _duckFlight = duckRoot.GetComponent<DuckFlightController>();
            _duckImpact = duckRoot.GetComponent<DuckImpact>();
        }

        var enemy = GetComponent<TargetEnemy>();
        if (enemy != null)
            enemy.OnDamaged += () => _isShooting = false;
    }

    private void Update()
    {
        if (!_isShooting) return;
        if (_duck == null || _duckFlight == null) return;
        if (!_duckFlight.IsLaunched) return;
        if (_duckImpact != null && _duckImpact.HasCrashed) return;

        // Rotate to face duck on Y axis only
        Vector3 dirToDuck = _duck.position - transform.position;
        dirToDuck.y = 0f;
        if (dirToDuck.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dirToDuck);

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            Fire();
            _fireTimer = fireRate;
        }
    }

    private void Fire()
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        var go   = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        var proj = go.GetComponent<BeaverProjectile>();
        if (proj != null)
            proj.Launch((_duck.position - spawnPoint.position).normalized, bulletSpeed);
    }
}