using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BruteAI : MonoBehaviour
{
    public float moveSpeed = 1.2f;
    public float pushForce = 8f;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (enemy.IsKnocked()) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player")) return;

        Rigidbody2D playerRb = col.collider.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector2 pushDir =
            (col.transform.position - transform.position).normalized;

        playerRb.AddForce(pushDir * pushForce, ForceMode2D.Impulse);

        CameraShake.ShakeCamera(0.1f, 0.15f);
    }
}
