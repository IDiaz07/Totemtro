using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArcProjectilePhysics : MonoBehaviour
{
    [Header("Projectile")]
    public float flightTime = 0.8f;
    public float gravityScale = 1.5f;
    public float impactRadius = 0.8f;
    public float damage = 12f;

    [Header("Spawn")]
    public GameObject puddlePrefab;

    Rigidbody2D rb;
    float timer;
    bool hasLanded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    public void Init(Vector2 target)
    {
        Vector2 start = transform.position;
        float gravity = Physics2D.gravity.y * gravityScale;

        float vx = (target.x - start.x) / flightTime;
        float vy = (target.y - start.y - 0.5f * gravity * flightTime * flightTime) / flightTime;

        rb.linearVelocity = new Vector2(vx, vy);
    }

    void Update()
    {
        if (hasLanded) return;

        timer += Time.deltaTime;

        // Rotación en el aire
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(
                rb.linearVelocity.y,
                rb.linearVelocity.x
            ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Cuando termina el vuelo → aterriza
        if (timer >= flightTime)
        {
            Land();
        }
    }

    void Land()
    {
        hasLanded = true;

        // 💥 Daño en área
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            impactRadius
        );

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Vector2 dir =
                    (hit.transform.position - transform.position).normalized;

                player.TakeDamage(damage, dir);
            }
        }

        // ☣ Crear slime
        if (puddlePrefab != null)
            Instantiate(puddlePrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // 🔍 Para visualizar el radio en editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
