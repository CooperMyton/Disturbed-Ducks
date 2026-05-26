using UnityEngine;

public class DuckFamilyMember : MonoBehaviour
{
    private DuckFamilyController _family;
    private bool _hasCrashed;

    public void Initialize(DuckFamilyController family)
    {
        _family = family;
        _hasCrashed = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasCrashed || _family == null) return;
        if (collision.gameObject.GetComponent<DuckFamilyMember>() != null) return;

        TargetEnemy enemy = collision.gameObject.GetComponentInParent<TargetEnemy>();
        if (enemy != null)
            enemy.TakeDamage(25f);

        Destructible destructible = collision.gameObject.GetComponentInParent<Destructible>();
        if (destructible != null)
            destructible.TakeDamage(25f);
        _hasCrashed = true;
        _family.OnFamilyDuckCrashed();
    }
}