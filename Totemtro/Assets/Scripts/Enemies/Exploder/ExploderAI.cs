using UnityEngine;

public class ExploderAI : MonoBehaviour
{
    public float speed = 4f;
    public float explosionRadius = 2f;
    public float explosionDamage = 25f;

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

        Vector2 dir =
            (player.position - transform.position).normalized;

        rb.linearVelocity = dir * speed;
    }

    void OnDestroy()
    {
        Explode();
    }

    void Explode()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
                player.TakeDamage(explosionDamage,
                    (hit.transform.position - transform.position).normalized);
        }

        CameraShake.Instance?.Shake(0.2f, 0.2f);
    }
}
