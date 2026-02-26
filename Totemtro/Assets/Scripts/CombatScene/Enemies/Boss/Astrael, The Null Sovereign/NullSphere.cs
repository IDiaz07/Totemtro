using UnityEngine;
using System.Collections;

public class NullSphere : MonoBehaviour
{
    [Header("Core Settings")]
    public float growSpeed = 2f;
    public float duration = 3f;
    public float explosionRadius = 2f;
    public float damage = 20f;
    public float moveSpeed = 2f;

    [Header("Gravity Effect")]
    public float pullRadius = 4f;
    public float pullStrength = 12f;
    public float slowPercent = 0.5f;

    [Header("Visual Size")]
    public float initialScale = 0.5f;

    Vector2 direction;
    float timer;
    Transform player;

    void Start()
    {
        transform.localScale = Vector3.one * initialScale;
    }

    public void Init(Vector2 target)
    {
        direction =
            (target - (Vector2)transform.position).normalized;

        player = GameObject
            .FindGameObjectWithTag("Player")
            ?.transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Movimiento
        transform.position +=
            (Vector3)(direction * moveSpeed * Time.deltaTime);

        // Crecimiento
        transform.localScale +=
            Vector3.one * growSpeed * Time.deltaTime;

        // NUEVA gravedad compatible con MovePosition
        ApplyGravitationalPull();

        if (timer >= duration)
            Explode();
    }

    void ApplyGravitationalPull()
    {
        if (player == null) return;

        float distance =
            Vector2.Distance(transform.position, player.position);

        if (distance > pullRadius) return;

        PlayerMovement movement =
            player.GetComponent<PlayerMovement>();

        if (movement == null) return;

        Vector2 dir =
            ((Vector2)transform.position - (Vector2)player.position);

        if (dir.sqrMagnitude < 0.01f) return;

        dir.Normalize();

        // Cuanto más cerca, más fuerte
        float strength =
            pullStrength * (1f - distance / pullRadius);

        movement.ApplyPull(dir * strength * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        PlayerMovement move =
            col.GetComponent<PlayerMovement>();

        if (move != null)
            move.ApplySlow(slowPercent, 1f);
    }

    void Explode()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius
            );

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth playerHealth =
                hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                Vector2 dir =
                    (hit.transform.position - transform.position).normalized;

                playerHealth.TakeDamage(damage, dir);
            }
        }

        Destroy(gameObject);
    }
}
