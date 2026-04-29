using UnityEngine;

/// <summary>
/// Contrail that fades out behind the duck during flight.
/// Uses Unity's built-in TrailRenderer.
/// Attach to a child of DuckRoot — not DuckRoot itself.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class FlightTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailTime = 0.6f;
    [SerializeField] private float startWidth = 0.4f;
    [SerializeField] private float endWidth = 0f;
    [SerializeField] private Color trailColor = new Color(1f, 1f, 1f, 0.8f);

    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        ConfigureTrail();
        _trail.emitting = false;
    }

    private void ConfigureTrail()
    {
        _trail.time       = trailTime;
        _trail.startWidth = startWidth;
        _trail.endWidth   = endWidth;
        _trail.startColor = trailColor;
        _trail.endColor   = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);

        // Use Sprites/Default so no extra material setup is needed
        _trail.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetEmitting(bool emitting) => _trail.emitting = emitting;

    public void Clear() => _trail.Clear();
}