using System.Collections.Generic;
using UnityEngine;

public class SplitController : MonoBehaviour
{
    [SerializeField] private GameObject miniDuckPrefab;

    private readonly List<GameObject> _miniDucks = new List<GameObject>();
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Split(float speed, float mass, int count, float angle)
    {
        Vector3 baseDir = _rb.linearVelocity.normalized;
        if (baseDir.sqrMagnitude < 0.1f)
            baseDir = transform.forward;

        float[] angles = count == 3
            ? new float[] { -angle, 0f, angle }
            : new float[] { -angle, angle };

        foreach (float a in angles)
        {
            Vector3 dir = Quaternion.AngleAxis(a, transform.up) * baseDir;
            SpawnMiniDuck(dir, speed, mass);
        }

        // Ignore collisions between mini ducks
        for (int i = 0; i < _miniDucks.Count; i++)
            for (int j = i + 1; j < _miniDucks.Count; j++)
                if (_miniDucks[i] != null && _miniDucks[j] != null)
                    Physics.IgnoreCollision(
                        _miniDucks[i].GetComponent<Collider>(),
                        _miniDucks[j].GetComponent<Collider>());
    }

    private void SpawnMiniDuck(Vector3 direction, float speed, float mass)
    {
        if (miniDuckPrefab == null) return;

        // Offset spawn slightly in spread direction so mini ducks don't overlap
        Vector3 spawnPos = transform.position + direction * 0.5f;

        var go = Instantiate(miniDuckPrefab, spawnPos, Quaternion.identity);
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass           = mass;
            rb.linearVelocity = direction * speed;
        }

        // Ignore collision with main duck
        var mainCollider = GetComponent<Collider>();
        var miniCollider = go.GetComponent<Collider>();
        if (mainCollider != null && miniCollider != null)
            Physics.IgnoreCollision(mainCollider, miniCollider);

        _miniDucks.Add(go);
    }

    public void OnReset()
    {
        foreach (var duck in _miniDucks)
            if (duck != null) Destroy(duck);
        _miniDucks.Clear();
    }
}