using UnityEngine;

public class MovingCar : MonoBehaviour
{
    public enum MovementMode
    {
        PingPongPath,
        LoopPath
    }

    [Header("Path")]
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private MovementMode movementMode = MovementMode.PingPongPath;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private float waypointReachDistance = 0.5f;
    [SerializeField] private int startingTargetIndex = 0;
    [Header("Damage")]
    [SerializeField] private float damage = 999f;

    [Header("Collision")]
    [SerializeField] private string[] unbreakableTags = { "Ground", "obstacle" };

    private Rigidbody _rb;
    private Vector3[] _pathPositions;
    private int _targetIndex;
    private int _direction = 1;
    private bool _stopped;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    private void Start()
    {
        CachePathPositions();

        if (_pathPositions == null || _pathPositions.Length == 0)
            return;

        _targetIndex = Mathf.Clamp(startingTargetIndex, 0, _pathPositions.Length - 1);
    }

    private void FixedUpdate()
    {
        if (_stopped) return;
        if (_pathPositions == null || _pathPositions.Length == 0) return;

        Vector3 current = _rb != null ? _rb.position : transform.position;
        Vector3 target = _pathPositions[_targetIndex];

        Vector3 toTarget = target - current;
        toTarget.y = 0f;

        if (toTarget.magnitude <= waypointReachDistance)
        {
            AdvanceTarget();
            return;
        }

        Vector3 direction = toTarget.normalized;
        Vector3 nextPosition = current + direction * speed * Time.fixedDeltaTime;

        Quaternion nextRotation = transform.rotation;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        if (_rb != null)
        {
            _rb.MovePosition(nextPosition);
            _rb.MoveRotation(nextRotation);
        }
        else
        {
            transform.SetPositionAndRotation(nextPosition, nextRotation);
        }
    }

    private void CachePathPositions()
    {
        if (pathPoints == null)
        {
            _pathPositions = new Vector3[0];
            return;
        }

        _pathPositions = new Vector3[pathPoints.Length];

        for (int i = 0; i < pathPoints.Length; i++)
            _pathPositions[i] = pathPoints[i] != null ? pathPoints[i].position : transform.position;
    }

    private int GetNearestPathIndex()
    {
        if (_pathPositions == null || _pathPositions.Length == 0) return 0;

        int nearest = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < _pathPositions.Length; i++)
        {
            float distance = Vector3.SqrMagnitude(_pathPositions[i] - transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = i;
            }
        }

        return nearest;
    }

    private void AdvanceTarget()
    {
        if (_pathPositions.Length <= 1) return;

        if (movementMode == MovementMode.LoopPath)
        {
            _targetIndex = (_targetIndex + 1) % _pathPositions.Length;
            return;
        }

        _targetIndex += _direction;

        if (_targetIndex >= _pathPositions.Length)
        {
            _targetIndex = _pathPositions.Length - 2;
            _direction = -1;
        }
        else if (_targetIndex < 0)
        {
            _targetIndex = 1;
            _direction = 1;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (HasUnbreakableTag(collision.gameObject))
        {
            _stopped = true;
            return;
        }

        collision.gameObject.GetComponent<TargetEnemy>()?.TakeDamage(damage);

        var destructible = collision.gameObject.GetComponent<Destructible>();
        if (destructible != null)
            destructible.TakeDamage(damage, false);
    }

    private bool HasUnbreakableTag(GameObject obj)
    {
        foreach (string tagName in unbreakableTags)
        {
            if (!string.IsNullOrEmpty(tagName) && obj.CompareTag(tagName))
                return true;
        }

        return false;
    }
}
