using UnityEngine;
using System.Collections;

public class NullSphere : MonoBehaviour
{
    public float growSpeed = 2f;
    public float duration = 3f;
    public float explosionRadius = 2f;
    public float slowPercent = 0.5f;
    public float damage = 20f;
    public float moveSpeed = 2f;

    Vector2 direction;
    float timer;

    public void Init(Vector2 target)
    {
        direction =
            (target - (Vector2)transform.position).normalized;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position +=
            (Vector3)(direction * moveSpeed * Time.deltaTime);

        transform.localScale +=
            Vector3.one * growSpeed * Time.deltaTime;

        if (timer >= duration)
            Explode();
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

            PlayerHealth player =
                hit.GetComponent<PlayerHealth>();

            if (player != null)
            {
                Vector2 dir =
                    (hit.transform.position - transform.position).normalized;

                player.TakeDamage(damage, dir);
            }
        }

        Destroy(gameObject);
    }
}
