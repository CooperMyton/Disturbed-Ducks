using UnityEngine;

public static class DuckHazardImmunity
{
    public static bool IsImmuneToLaserOrNet(Collider other)
    {
        if (other == null) return false;

        var shield = other.GetComponentInParent<ShieldController>();
        if (shield != null && shield.IsActive)
            return true;

        var ghost = other.GetComponentInParent<GhostPhaseController>();
        if (ghost != null && ghost.IsPhasing)
            return true;

        return false;
    }
}