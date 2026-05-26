using UnityEngine;

public class LaserHazard : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer laserRenderer;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float emissionIntensity = 4f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (laserRenderer == null)
            laserRenderer = GetComponentInChildren<Renderer>();

        if (laserRenderer != null)
        {
            laserRenderer.material.color = laserColor;
            laserRenderer.material.EnableKeyword("_EMISSION");
            laserRenderer.material.SetColor("_EmissionColor", laserColor * emissionIntensity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCrashDuck(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryCrashDuck(other);
    }

    private void TryCrashDuck(Collider other)
    {
        var impact = other.GetComponentInParent<DuckImpact>();
        if (impact == null || impact.HasCrashed)
            return;

        if (DuckHazardImmunity.IsImmuneToLaserOrNet(other))
            return;

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

        impact.Crash();
    }
}