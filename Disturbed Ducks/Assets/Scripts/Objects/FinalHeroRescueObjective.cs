using UnityEngine;

public class FinalHeroRescueObjective : MonoBehaviour
{
    [SerializeField] private GameObject captiveVisual;

    private bool _rescued;

    public bool IsRescued => _rescued;
    public event System.Action OnRescued;

    private void OnTriggerEnter(Collider other)
    {
        if (_rescued) return;

        var duck = other.GetComponentInParent<DuckImpact>();
        if (duck == null) return;

        _rescued = true;

        if (captiveVisual != null)
            captiveVisual.SetActive(false);

        OnRescued?.Invoke();
    }
}