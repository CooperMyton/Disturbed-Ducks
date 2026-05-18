using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    private Coroutine _shieldRoutine;
    private bool _isActive;
    private float _damageMultiplier = 1f;

    public bool IsActive => _isActive;
    public float DamageMultiplier => _isActive ? _damageMultiplier : 1f;

    public void Activate(float duration, float damageMultiplier)
    {
        if (_shieldRoutine != null)
            StopCoroutine(_shieldRoutine);

        _shieldRoutine = StartCoroutine(ShieldRoutine(duration, damageMultiplier));
    }

    public void ForceEnd()
    {
        if (_shieldRoutine != null)
        {
            StopCoroutine(_shieldRoutine);
            _shieldRoutine = null;
        }

        _isActive = false;
        _damageMultiplier = 1f;
    }

    private IEnumerator ShieldRoutine(float duration, float damageMultiplier)
    {
        _isActive = true;
        _damageMultiplier = Mathf.Max(1f, damageMultiplier);

        yield return new WaitForSeconds(duration);

        ForceEnd();
    }
}
