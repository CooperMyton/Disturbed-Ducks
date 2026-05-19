using UnityEngine;

public class WindGustEmitter : MonoBehaviour
{
    [Header("Gust")]
    [SerializeField] private WindGust gustPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Timing")]
    [SerializeField] private bool fireAutomatically = true;
    [SerializeField] private float fireInterval = 3f;
    [SerializeField] private float firstFireDelay = 0.5f;

    private float _timer;

    private void Start()
    {
        _timer = firstFireDelay;
    }

    private void Update()
    {
        if (!fireAutomatically) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        Fire();
        _timer = fireInterval;
    }

    public void Fire()
    {
        if (gustPrefab == null) return;

        Transform origin = spawnPoint != null ? spawnPoint : transform;
        Instantiate(gustPrefab, origin.position, origin.rotation);
    }
}