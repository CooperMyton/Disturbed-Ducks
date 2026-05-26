using UnityEngine;

public class SecurityNetProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector3 _velocity;
    private bool _launched;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed)
    {
        _velocity = direction.normalized * speed;
        _launched = true;
    }

    private void Update()
    {
        if (!_launched) return;

        transform.position += _velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        var impact = other.GetComponentInParent<DuckImpact>();
        if (impact == null || impact.HasCrashed) return;

        if (DuckHazardImmunity.IsImmuneToLaserOrNet(other))
        {
            Destroy(gameObject);
            return;
        }

        impact.Crash();
        Destroy(gameObject);
    }
}