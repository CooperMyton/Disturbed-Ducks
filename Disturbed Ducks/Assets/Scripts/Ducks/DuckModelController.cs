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

    private void SwapTo(GameObject prefab)
    {
        if (_activeModel != null)
            Destroy(_activeModel);

        if (prefab == null) return;

        _activeModel = Instantiate(prefab, transform);
        _activeModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}