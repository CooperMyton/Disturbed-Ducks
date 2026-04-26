using UnityEngine;

[CreateAssetMenu(fileName = "DashAbility", menuName = "Abilities/Dash Ability")]
public class DashAbility : AbilityBase
{
    [Header("Dash Settings")]
    [Tooltip("Flat speed added on dash — can push over max speed")]
    public float speedBoost = 15f;

    public override void Use(GameObject user)
    {
        DuckFlightController flight = user.GetComponent<DuckFlightController>();
        if (flight == null) return;

        flight.ApplySpeedBoost(speedBoost);
    }
}