using UnityEngine;

public class SecurityNetLauncher : MonoBehaviour
{
    [Header("Launch")]
    [SerializeField] private GameObject netProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float launchInterval = 4f;
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float netSpeed = 18f;

    [Header("Warning Visual")]
    [SerializeField] private GameObject warningObject;
    [SerializeField] private Light warningLight;

    private float _timer;
    private bool _warningActive;

    private void Start()
    {
        _timer = launchInterval;
        SetWarning(false);
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (!_warningActive && _timer <= warningTime)
        {
            _warningActive = true;
            SetWarning(true);
        }

        if (_timer <= 0f)
        {
            FireNet();
            _timer = launchInterval;
            _warningActive = false;
            SetWarning(false);
        }
    }

    private void FireNet()
    {
        if (netProjectilePrefab == null || firePoint == null) return;

        GameObject net = Instantiate(
            netProjectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        var projectile = net.GetComponent<SecurityNetProjectile>();
        if (projectile != null)
            projectile.Launch(firePoint.forward, netSpeed);
    }

    private void SetWarning(bool active)
    {
        if (warningObject != null)
            warningObject.SetActive(active);

        if (warningLight != null)
            warningLight.enabled = active;
    }
}