using UnityEngine;

public class BodySwayEnemy : MonoBehaviour
{
    public float maxRotation = 5f;
    public float swaySpeed = 6f;
    public float smooth = 10f;

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

        float speed = rb.linearVelocity.magnitude;

        float targetZ = 0f;

        if (speed > 0.1f)
        {
            targetZ = Mathf.Sin(Time.time * swaySpeed) * maxRotation;
        }

        float currentZ = transform.localEulerAngles.z;

        float z = Mathf.LerpAngle(
            currentZ,
            targetZ,
            Time.deltaTime * smooth
        );

        transform.localEulerAngles = new Vector3(
            originalRotation.x,
            originalRotation.y,
            z
        );
    }
}
