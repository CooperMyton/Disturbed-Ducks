using UnityEngine;

/// Swaps duck model based on state.
/// Instantiates models as children of this transform.
/// Drag model prefabs into DuckDefinition — no code changes per duck.

public class DuckModelController : MonoBehaviour
{
    private DuckDefinition _definition;
    private GameObject _activeModel;

    public void SetDefinition(DuckDefinition definition)
    {
        _definition = definition;
        SetNeutral();
    }

    public void SetNeutral()  => SwapTo(_definition?.neutralModel);
    public void SetFlight()   => SwapTo(_definition?.flightModel);
    public void SetCrashed()  => SwapTo(_definition?.crashedModel);

    /// Tints all renderers on the active model. Call each frame during a lerp
    /// to drive a smooth color transition (e.g. bomb countdown → red).
    /// Operates on material instances so other ducks are unaffected.
    /// Falls back to searching this transform's children when no prefab model
    /// is assigned (e.g. placeholder cylinder directly in the hierarchy).
    public void SetTint(Color color)
    {
        var root = _activeModel != null ? _activeModel.transform : transform;
        foreach (var r in root.GetComponentsInChildren<Renderer>())
            r.material.color = color;
    }

    /// Resets all renderer tints to white (no tint).
    public void ClearTint()
    {
        SetTint(Color.white);
    }

    private void SwapTo(GameObject prefab)
    {
        if (_activeModel != null)
            Destroy(_activeModel);

        if (prefab == null) return;

        _activeModel = Instantiate(prefab, transform);
        _activeModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}