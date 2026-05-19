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

        _hasCrashed = true;
        _family.OnFamilyDuckCrashed();
    }
}