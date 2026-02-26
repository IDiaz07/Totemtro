using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );

        // 🔑 SNAP A PIXEL (evita tembleque)
        float pixelsPerUnit = 64f;
        smoothPos.x = Mathf.Round(smoothPos.x * pixelsPerUnit) / pixelsPerUnit;
        smoothPos.y = Mathf.Round(smoothPos.y * pixelsPerUnit) / pixelsPerUnit;

        transform.position = smoothPos;
    }
}
