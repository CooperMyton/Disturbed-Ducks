using System.Collections.Generic;
using UnityEngine;

public class GhostPhaseController : MonoBehaviour
{
    [SerializeField] private LayerMask phaseableLayers = ~0;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float clearanceCheckPadding = 0.02f;

    private Collider _duckCollider;
    private readonly List<Collider> _ignoredColliders = new List<Collider>();
    private bool _waitingForClearance;

    private void Awake()
    {
        _duckCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!_waitingForClearance) return;

        if (CanBecomeSolid())
            BecomeSolid();
    }

    public void BeginPhase()
    {
        _waitingForClearance = false;
        IgnorePhaseableColliders(true);
        GetComponent<DuckModelController>()?.SetAbility();
    }

    public void EndPhase()
    {
        if (CanBecomeSolid())
            BecomeSolid();
        else
            _waitingForClearance = true;
    }

    public void ForceEndPhase()
    {
        _waitingForClearance = false;
        RestoreIgnoredCollisions();
        GetComponent<DuckModelController>()?.SetFlight();
    }

    private void IgnorePhaseableColliders(bool ignore)
    {
        if (_duckCollider == null) return;

        RestoreIgnoredCollisions();

        foreach (var other in FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (other == null) continue;
            if (other == _duckCollider) continue;
            if (other.transform.IsChildOf(transform)) continue;
            if (other.CompareTag(groundTag)) continue;

            bool layerAllowed = (phaseableLayers.value & (1 << other.gameObject.layer)) != 0;
            if (!layerAllowed) continue;

            Physics.IgnoreCollision(_duckCollider, other, ignore);
            _ignoredColliders.Add(other);
        }
    }

    private bool CanBecomeSolid()
    {
        if (_duckCollider == null) return true;

        Bounds bounds = _duckCollider.bounds;
        Vector3 halfExtents = bounds.extents + Vector3.one * clearanceCheckPadding;

        Collider[] overlaps = Physics.OverlapBox(
            bounds.center,
            halfExtents,
            transform.rotation,
            phaseableLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in overlaps)
        {
            if (hit == null) continue;
            if (hit == _duckCollider) continue;
            if (hit.transform.IsChildOf(transform)) continue;
            if (hit.CompareTag(groundTag)) continue;

            return false;
        }

        return true;
    }

    private void BecomeSolid()
    {
        _waitingForClearance = false;
        RestoreIgnoredCollisions();
        GetComponent<DuckModelController>()?.SetFlight();
    }

    private void RestoreIgnoredCollisions()
    {
        if (_duckCollider == null) return;

        foreach (var other in _ignoredColliders)
        {
            if (other != null)
                Physics.IgnoreCollision(_duckCollider, other, false);
        }

        _ignoredColliders.Clear();
    }
}
