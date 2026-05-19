using UnityEngine;

public class WindGust : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float duration = 2f;

    [Header("Push")]
    [SerializeField] private float pushPower = 18f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float visualSpinSpeed = 180f;

    private float _age;

    private void Start()
    {
        if (visualRoot == null)
            visualRoot = transform;

        transform.localScale = Vector3.one * radius;
    }

    private void Update()
    {
        _age += Time.deltaTime;

        transform.position += transform.forward * speed * Time.deltaTime;

        if (visualRoot != null)
            visualRoot.Rotate(Vector3.forward, visualSpinSpeed * Time.deltaTime, Space.Self);

        if (_age >= duration)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var hit in hits)
        {
            var duckImpact = hit.GetComponent<DuckImpact>();
            if (duckImpact == null || duckImpact.HasCrashed) continue;
            if (IsImmuneToWind(hit.gameObject)) continue;

            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            float distance = Vector3.Distance(transform.position, rb.position);
            float t = Mathf.Clamp01(distance / radius);
            float strength = pushPower * falloff.Evaluate(t);

            rb.AddForce(transform.forward * strength, ForceMode.Acceleration);
        }
    }

    private bool IsImmuneToWind(GameObject duck)
    {
        var def = duck.GetComponent<DuckController>()?.Definition;
        if (def != null && def.disableFlightControls)
            return true;

        var shield = duck.GetComponent<ShieldController>();
        if (shield != null && shield.IsActive)
            return true;

        var anvil = duck.GetComponent<AnvilSlamController>();
        if (anvil != null && anvil.BlocksWind())
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * radius * 2f);
    }
}