using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StalkerAI : MonoBehaviour
{
    public float speed = 4f;
    public float directionChangeInterval = 0.4f;
    public float randomnessAmount = 0.6f;

    Transform player;
    Rigidbody2D rb;

    Vector2 currentDirection;
    float changeTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        changeTimer = directionChangeInterval;
    }

    void Update()
    {
        if (player == null) return;

        changeTimer -= Time.deltaTime;

        if (changeTimer <= 0f)
        {
            UpdateDirection();
            changeTimer = directionChangeInterval;
        }
    }

    void FixedUpdate()
    {
        if (GetComponent<Enemy>().IsKnocked()) return;
        rb.linearVelocity = currentDirection * speed;
    }

    void UpdateDirection()
    {
        Vector2 toPlayer =
            (player.position - transform.position).normalized;

        // Ruido direccional
        Vector2 randomOffset = Random.insideUnitCircle * randomnessAmount;

        currentDirection = (toPlayer + randomOffset).normalized;
    }
}
