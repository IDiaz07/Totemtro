using UnityEngine;

public class BodyTiltEnemy : MonoBehaviour
{
    public float maxTiltAngle = 8f;
    public float tiltSmooth = 8f;

    Rigidbody2D rb;
    Vector3 originalRotation;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        originalRotation = transform.localEulerAngles;
    }

    void Update()
    {
        if (rb == null) return;

        Vector2 velocity = rb.linearVelocity;

        float targetZ = 0f;

        if (velocity.magnitude > 0.1f)
        {
            targetZ = -velocity.x * maxTiltAngle;
        }

        float z = Mathf.LerpAngle(
            transform.localEulerAngles.z,
            targetZ,
            Time.deltaTime * tiltSmooth
        );

        transform.localEulerAngles = new Vector3(
            originalRotation.x,
            originalRotation.y,
            z
        );
    }
}
