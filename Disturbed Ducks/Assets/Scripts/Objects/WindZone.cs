using UnityEngine;

public class WindZone : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] private float range = 18f;
    [SerializeField] private float coneAngle = 35f;

    [Header("Push")]
    [SerializeField] private float windSpeed = 12f;
    [SerializeField] private AnimationCurve falloff = AnimationCurve.Linear(0f, 1f, 1f, 0.35f);

    private void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        foreach (Collider hit in hits)
        {
            DuckFlightController flight = hit.GetComponentInParent<DuckFlightController>();
            DuckImpact impact = hit.GetComponentInParent<DuckImpact>();

            if (flight == null || impact == null || impact.HasCrashed)
                continue;

            GameObject duck = flight.gameObject;
            if (IsImmuneToWind(duck))
                continue;

            Vector3 toDuck = flight.transform.position - transform.position;
            float distance = toDuck.magnitude;

            if (distance <= 0.01f)
                continue;

            Vector3 directionToDuck = toDuck.normalized;
            float angle = Vector3.Angle(transform.forward, directionToDuck);

            if (angle > coneAngle)
                continue;

            float distanceT = Mathf.Clamp01(distance / range);
            float strength = windSpeed * falloff.Evaluate(distanceT);

            flight.AddWindVelocity(transform.forward * strength);
        }
    }

    private bool IsImmuneToWind(GameObject duck)
    {
        DuckController controller = duck.GetComponent<DuckController>();
        if (controller != null && controller.Definition != null && controller.Definition.disableFlightControls)
            return true;

        ShieldController shield = duck.GetComponent<ShieldController>();
        if (shield != null && shield.IsActive)
            return true;

        AnvilSlamController anvil = duck.GetComponent<AnvilSlamController>();
        if (anvil != null && anvil.BlocksWind())
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * range);
    }
}