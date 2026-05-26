using UnityEngine;
using UnityEngine.UI;

public class SimpleWorldHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera != null)
            transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
    }

    public void SetValue(float normalized)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(normalized);
    }

    public void SetColor(Color color)
    {
        if (fillImage != null)
            fillImage.color = color;
    }
}