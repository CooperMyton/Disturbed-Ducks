using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Transform duckRoot;
    [SerializeField] private float positionSmoothing = 10f;
    [SerializeField] private float rotationSmoothing = 8f;

    private float _smoothedYaw;
    private bool _frozen = false; // stops yaw updates after a crash

    private void Awake()
    {
        _smoothedYaw = duckRoot.eulerAngles.y;
    }

    private void LateUpdate()
    {
        // Always follow position
        transform.position = Vector3.Lerp(
            transform.position,
            duckRoot.position,
            positionSmoothing * Time.deltaTime
        );

        if (_frozen) return;

        // Derive yaw from forward vector instead of eulerAngles.y
        // This is immune to gimbal lock and tumble-spinning
        Vector3 flatForward = new Vector3(duckRoot.forward.x, 0f, duckRoot.forward.z);

        // Only update if the duck has meaningful horizontal direction
        // (avoids jitter when pointing straight up or down)
        if (flatForward.sqrMagnitude > 0.01f)
        {
            float targetYaw = Quaternion.LookRotation(flatForward).eulerAngles.y;
            _smoothedYaw = Mathf.LerpAngle(_smoothedYaw, targetYaw, rotationSmoothing * Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(0f, _smoothedYaw, 0f);
    }

    // Call this when the duck crashes so the camera holds its last good angle
    public void FreezeYaw() => _frozen = true;

    // Call this when resetting the duck for the next launch
    public void UnfreezeYaw() => _frozen = false;
}