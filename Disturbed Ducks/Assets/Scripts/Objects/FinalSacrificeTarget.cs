using UnityEngine;

public class FinalSacrificeTarget : MonoBehaviour
{
    [SerializeField] private FinalBossPhaseManager finalManager;
    [SerializeField] private BlackHoleFinale blackHoleFinale;

    private bool _triggered;

    private void OnCollisionEnter(Collision collision)
    {
        TryStartFinale(collision.rigidbody);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryStartFinale(other.GetComponentInParent<Rigidbody>());
    }

    private void TryStartFinale(Rigidbody rb)
    {
        if (_triggered) return;
        if (finalManager == null || !finalManager.PartTwoStarted) return;
        if (rb == null) return;

        var duck = rb.GetComponent<DuckController>();
        if (duck == null || duck.Definition != finalManager.HeroKingDuckDefinition)
            return;

        _triggered = true;

        rb.GetComponent<DuckImpact>()?.Crash();
        blackHoleFinale?.Begin();
    }
}