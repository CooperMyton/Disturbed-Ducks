using UnityEngine;

public class ProjectileDuckShooter : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float spawnForwardOffset = 1.2f;

    private GameObject _projectilePrefab;
    private int _maxAmmo = 3;
    private int _currentAmmo = 3;
    private float _damage = 40f;
    private float _projectileSpeed = 50f;

    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _maxAmmo;

    public void Configure(GameObject projectilePrefab, int maxAmmo, float damage, float projectileSpeed)
    {
        _projectilePrefab = projectilePrefab;
        _maxAmmo = Mathf.Max(0, maxAmmo);
        _damage = damage;
        _projectileSpeed = projectileSpeed;

        if (_currentAmmo > _maxAmmo)
            _currentAmmo = _maxAmmo;
    }

    public void OnLaunched()
    {
        _currentAmmo = _maxAmmo;
        AbilityUI.Instance?.SetAmmo(_currentAmmo, _maxAmmo);
    }

    public void OnReset()
    {
        _currentAmmo = _maxAmmo;
    }

    public void TryShoot()
    {
        if (_projectilePrefab == null) return;

        if (_currentAmmo <= 0)
        {
            AbilityUI.Instance?.SetAmmo(_currentAmmo, _maxAmmo);
            return;
        }

        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            direction = transform.root.forward;

        direction = direction.normalized;
        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + direction * spawnForwardOffset;

        GameObject projectileObject = Instantiate(
            _projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        var projectile = projectileObject.GetComponent<DuckProjectile>();
        if (projectile != null)
            projectile.Launch(direction, _projectileSpeed, _damage);

        _currentAmmo--;

        AbilityUI.Instance?.SetAmmo(_currentAmmo, _maxAmmo);
    }
}