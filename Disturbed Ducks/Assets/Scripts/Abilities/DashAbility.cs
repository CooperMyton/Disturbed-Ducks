using UnityEngine;


/// Dash ability for the basic duck.
/// Briefly multiplies forward speed then returns to normal.
/// Create asset via Assets > Create > Abilities > Dash Ability.
[CreateAssetMenu(fileName = "DashAbility", menuName = "Abilities/Dash Ability")]
public class DashAbility : AbilityBase
{
    [Header("Dash Settings")]
    [Tooltip("Speed multiplier during the dash")]
    public float dashMultiplier = 2.5f;

    [Tooltip("How long the dash lasts in seconds")]
    public float dashDuration = 0.4f;

    public override void Use(GameObject user)
    {
        AbilityController controller = user.GetComponent<AbilityController>();
        if (controller == null) return;

        controller.StartCoroutine(DashRoutine(user));
    }

    private System.Collections.IEnumerator DashRoutine(GameObject user)
    {
        DuckFlightController flight = user.GetComponent<DuckFlightController>();
        if (flight == null) yield break;

        flight.ApplySpeedMultiplier(dashMultiplier);
        yield return new WaitForSeconds(dashDuration);
        flight.ApplySpeedMultiplier(1f / dashMultiplier); // restore
    }
}