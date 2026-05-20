using UnityEngine;

/// Adds left/right pacing to any enemy.
/// Add alongside TargetEnemy — does not replace it.
/// Sets the Rigidbody to kinematic so the enemy stays on its path.
public class PacingEnemy : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float moveSpeed      = 2f;
    [SerializeField] private float pauseDuration  = 0.8f;

    private Vector3  _pointA;
    private Vector3  _pointB;
    private Vector3  _target;
    private bool     _isPausing  = false;
    private float    _pauseTimer = 0f;

    private void Start()
    {
        // Patrol along world X axis — adjust Vector3.right to
        // Vector3.forward if your stage runs along the Z axis instead
        Vector3 strafeDirection = transform.right;
        strafeDirection.y = 0f;

        if (strafeDirection.sqrMagnitude < 0.01f)
            strafeDirection = Vector3.right;
        else
            strafeDirection.Normalize();

        _pointA = transform.position - strafeDirection * patrolDistance;
        _pointB = transform.position + strafeDirection * patrolDistance;
        _target = _pointB;

        // Kinematic so the beaver stays on its path and isn't
        // knocked off course mid-patrol. TargetEnemy still receives
        // collision damage and destroys the GO on death as normal.
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    private void Update()
    {
        if (_isPausing)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f) _isPausing = false;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position, _target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _target) < 0.05f)
        {
            _target     = (_target == _pointA) ? _pointB : _pointA;
            _isPausing  = true;
            _pauseTimer = pauseDuration;
        }
    }
}