using UnityEngine;

[CreateAssetMenu(fileName = "BombExplosionAbility",
                menuName = "Abilities/Bomb Explosion Ability")]
public class BombExplosionAbility : AbilityBase
{
    // IsSingleUse = true tells AbilityController to skip the normal cooldown UI.
    // AbilityUI will instead receive OnBombArmed() directly from ExplosionOnCrash
    // with the actual detonation delay.
    public override bool IsSingleUse => true;

    // Base stats live in ExplosionDefinition on the duck — nothing needed here.
    // Upgrade boosts come from AbilityController's accumulated values.
    public override void Use(GameObject user, float upgradeBoost)
    {
        var explosion = user.GetComponent<ExplosionOnCrash>();
        if (explosion == null)
        {
            Debug.LogWarning("BombExplosionAbility: ExplosionOnCrash not found on duck");
            return;
        }

        var controller = user.GetComponent<AbilityController>();
        explosion.StartAbilityCountdown(
            controller?.RadiusBoost    ?? 0f,
            controller?.DamageBoost    ?? 0f,
            controller?.DelayReduction ?? 0f);

        // Note: AbilityUI.OnBombArmed is called inside StartAbilityCountdown,
        // after the detonation delay is computed. AbilityController skips its
        // normal OnAbilityUsed call when IsSingleUse is true.
    }
}