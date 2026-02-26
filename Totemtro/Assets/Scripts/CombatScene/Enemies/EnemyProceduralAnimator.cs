using UnityEngine;

public class EnemyProceduralAnimator : MonoBehaviour
{
    [Header("Idle")]
    public float idleBreathAmount = 0.02f;
    public float idleBreathSpeed = 2f;

    public float idleBobAmount = 0.01f;
    public float idleBobSpeed = 2f;

    [Header("Movement Bounce")]
    public float moveBounceHeight = 0.08f;
    public float moveBounceSpeed = 10f;

    public float squashAmount = 0.08f;

    [Header("Tilt & Sway")]
    public float tiltStrength = 6f;
    public float swayAmount = 4f;
    public float swaySpeed = 8f;

    public bool isHeavy = false;

    Rigidbody2D rb;

    Vector3 baseScale;
    Vector3 basePos;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        baseScale = transform.localScale;
        basePos = transform.localPosition;
    }

    void Update()
    {
        if (rb == null) return;

        float speed = rb.linearVelocity.magnitude;

        if (speed < 0.1f)
        {
            AnimateIdle();
        }
        else
        {
            AnimateMove(speed);
        }
    }

    void AnimateIdle()
    {
        float breath =
            Mathf.Sin(Time.time * idleBreathSpeed) * idleBreathAmount;

        float bob =
            Mathf.Sin(Time.time * idleBobSpeed) * idleBobAmount;

        transform.localScale = baseScale * (1f + breath);
        transform.localPosition = basePos + Vector3.up * bob;

        transform.localRotation = Quaternion.identity;
    }

    void AnimateMove(float speed)
    {
        float bounce =
            Mathf.Sin(Time.time * moveBounceSpeed * speed) * moveBounceHeight;

        transform.localPosition = basePos + Vector3.up * bounce;

        float squash =
            Mathf.Abs(Mathf.Sin(Time.time * moveBounceSpeed)) * squashAmount;

        transform.localScale = new Vector3(
            baseScale.x * (1f + squash),
            baseScale.y * (1f - squash),
            baseScale.z
        );

        Vector2 velocity = rb.linearVelocity;

        float targetTilt = -velocity.x * tiltStrength;

        float tilt = Mathf.LerpAngle(
            transform.localEulerAngles.z,
            targetTilt,
            Time.deltaTime * 6f
        );

        float sway = isHeavy ? 0f : Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        float finalZ = tilt + sway;


        transform.localRotation =
            Quaternion.Lerp(
                transform.localRotation,
                Quaternion.Euler(0, 0, finalZ),
                Time.deltaTime * 10f
            );
    }
}
