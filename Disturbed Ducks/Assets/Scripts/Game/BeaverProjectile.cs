using UnityEngine;

/// Fired by ShootingBeaver. Calls Crash() on the duck on contact.
/// Requires: Rigidbody (Is Kinematic on), SphereCollider (Is Trigger on)
public class BeaverProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector3 _velocity;
    private bool _launched = false;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed)
    {
        _velocity = direction * speed;
        _launched = true;
    }

    private void Update()
    {
        if (!_launched) return;
        transform.position += _velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        var impact = other.GetComponent<DuckImpact>();
        if (impact == null) return;
        if (impact.HasCrashed) return;
        var shield = other.GetComponent<ShieldController>();
        if (shield != null && shield.IsActive)
        {
            Destroy(gameObject);
            return;
        }


        impact.Crash();
        Destroy(gameObject);
    }
}