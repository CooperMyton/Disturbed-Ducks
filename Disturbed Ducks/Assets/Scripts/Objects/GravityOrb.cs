using UnityEngine;

public class GravityOrb : MonoBehaviour
{
    [Header("Gravity Effect")]
    [SerializeField] private float gravityMultiplier = 8f;
    [SerializeField] private float downwardSpeed = 40f;

    [Header("Visual")]
    [SerializeField] private Renderer orbRenderer;
    [SerializeField] private Color glowColor = new Color(0.65f, 0.15f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 5f;
    [SerializeField] private Light glowLight;
    [SerializeField] private float lightRange = 5f;
    [SerializeField] private float lightIntensity = 4f;

    [Header("Audio")]
    [SerializeField] private AudioClip touchSound;

    private bool _used = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (orbRenderer == null)
            orbRenderer = GetComponentInChildren<Renderer>();

        if (orbRenderer != null)
        {
            orbRenderer.material.color = glowColor;
            orbRenderer.material.EnableKeyword("_EMISSION");
            orbRenderer.material.SetColor("_EmissionColor", glowColor * emissionIntensity);
        }

        if (glowLight == null)
            glowLight = GetComponentInChildren<Light>();

        if (glowLight != null)
        {
            glowLight.color = glowColor;
            glowLight.range = lightRange;
            glowLight.intensity = lightIntensity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_used) return;

        var flight = other.GetComponentInParent<DuckFlightController>();
        var impact = other.GetComponentInParent<DuckImpact>();

        if (flight == null || impact == null || impact.HasCrashed)
            return;

        _used = true;

        flight.ApplyGravityOrb(gravityMultiplier, downwardSpeed);

        if (touchSound != null)
            AudioSource.PlayClipAtPoint(touchSound, transform.position);

        Destroy(gameObject);
    }
}