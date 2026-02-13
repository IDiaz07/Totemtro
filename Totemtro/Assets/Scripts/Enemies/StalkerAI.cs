using UnityEngine;

public class StalkerAI : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float directionChangeSpeed = 6f;

    Transform player;
    Rigidbody2D rb;

    Vector2 currentDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 desiredDirection =
            (player.position - transform.position).normalized;

        currentDirection = Vector2.Lerp(
            currentDirection,
            desiredDirection,
            directionChangeSpeed * Time.fixedDeltaTime
        );

        rb.linearVelocity = currentDirection.normalized * moveSpeed;
    }
}
