using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ArenaGuardian : MonoBehaviour
{
    Rigidbody2D rb;
    bool isProjectile;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void MoveTo(Vector2 target)
    {
        StartCoroutine(MoveRoutine(target));
    }

    IEnumerator MoveRoutine(Vector2 target)
    {
        Vector2 start = transform.position;
        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            transform.position =
                Vector2.Lerp(start, target, t / duration);

            float bob =
                Mathf.Sin(Time.time * 3f) * 0.3f;

            transform.position += Vector3.up * bob;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = target;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player")) return;

        PlayerHealth player =
            col.collider.GetComponent<PlayerHealth>();

        if (player != null)
        {
            Vector2 dir =
                (col.transform.position - transform.position).normalized;

            player.TakeDamage(10f, dir);
        }
    }

    void OnDestroy()
    {
        ArenaController arena = GetComponentInParent<ArenaController>();

        if (arena != null)
            arena.RemoveGuardian(this);
    }
}
