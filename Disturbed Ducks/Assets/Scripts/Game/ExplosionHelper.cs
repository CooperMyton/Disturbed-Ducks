using UnityEngine;
using System.Collections;

public static class ExplosionHelper
{
    /// Deals damage to everything with health in radius and shows a brief visual.
    public static void Trigger(Vector3 position, float radius, float damage,
        ExplosionDefinition def, MonoBehaviour runner)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius);
        foreach (var hit in hits)
        {
            // fromExplosion: true — allows explosion-only destructibles to take damage
            hit.GetComponent<Destructible>()?.TakeDamage(damage, true);
            hit.GetComponent<TargetEnemy>()?.TakeDamage(damage);
            hit.GetComponent<ExplosiveCar>()?.TakeDamage(damage, true);
            hit.GetComponentInParent<BossGenerator>()?.TakeDamage(damage);
            hit.GetComponentInParent<MechaHuskyBoss>()?.TakeDamage(damage);

        }

        Debug.Log($"Explosion at {position} — radius: {radius:F1}, " +
                $"damage: {damage:F1}, targets hit: {hits.Length}");

        runner.StartCoroutine(ExplosionVisual(position, radius, def));
    }

    private static IEnumerator ExplosionVisual(Vector3 center, float radius,
        ExplosionDefinition def)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.Destroy(sphere.GetComponent<Collider>());
        sphere.transform.position   = center;
        sphere.transform.localScale = Vector3.one * 0.1f;

        var rend = sphere.GetComponent<Renderer>();
        if (def.explosionMaterial != null)
        {
            rend.material = new Material(def.explosionMaterial);
        }
        else
        {
            // Fallback: tint the default material rather than calling Shader.Find.
            // Shader.Find returns null in WebGL builds for any shader not explicitly
            // included, which causes an invisible explosion.
            rend.material.color = def.explosionColor;
        }

        float elapsed  = 0f;
        float duration = def.visualDuration;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            sphere.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, radius * 2.2f, t);
            yield return null;
        }

        Object.Destroy(sphere);
    }
}