using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyOrganicChase : MonoBehaviour
{
    [Header("Speed")]
    public float maxSpeed = 4f;
    public float sprintSpeed = 6f;
    public float sprintDistance = 5f;

    public float acceleration = 25f;
    public float deceleration = 18f;

    [Header("Randomness")]
    public float randomnessAmount = 0.2f;

    [Header("Separation")]
    public float separationRadius = 1.2f;
    public float separationStrength = 2f;

    Rigidbody2D rb;
    Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.IsKnocked()) return;

        Vector2 toPlayer =
            (player.position - transform.position);

        float distance = toPlayer.magnitude;

        Vector2 desiredDirection =
            toPlayer.normalized +
            Random.insideUnitCircle * randomnessAmount;

        Vector2 separation = GetSeparationForce();

        Vector2 finalDir =
            (desiredDirection + separation).normalized;

        float targetSpeed =
            (distance > sprintDistance)
            ? sprintSpeed
            : maxSpeed;

        ApplyMovement(finalDir, targetSpeed);
    }

    void ApplyMovement(Vector2 direction, float targetSpeed)
    {
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 targetVelocity = direction * targetSpeed;

        Vector2 velocityDiff = targetVelocity - currentVelocity;

        Vector2 movement =
            Vector2.ClampMagnitude(
                velocityDiff,
                acceleration * Time.fixedDeltaTime
            );

        rb.linearVelocity += movement;
    }

    Vector2 GetSeparationForce()
    {
        Collider2D[] neighbors =
            Physics2D.OverlapCircleAll(
                transform.position,
                separationRadius
            );

        Vector2 force = Vector2.zero;

        foreach (var n in neighbors)
        {
            if (!n.CompareTag("Enemy")) continue;
            if (n.gameObject == gameObject) continue;

            Vector2 diff =
                (Vector2)(transform.position - n.transform.position);

            force += diff.normalized / Mathf.Max(diff.magnitude, 0.1f);
        }

        return force * separationStrength;
    }
}
