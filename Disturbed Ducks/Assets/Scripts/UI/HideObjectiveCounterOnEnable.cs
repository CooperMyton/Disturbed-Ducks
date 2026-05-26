using UnityEngine;

public class HideObjectiveCounterOnEnable : MonoBehaviour
{
    [SerializeField] private GameObject objectiveCounterObject;

    private void OnEnable()
    {
        if (objectiveCounterObject != null)
            objectiveCounterObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (objectiveCounterObject != null)
            objectiveCounterObject.SetActive(true);
    }
}