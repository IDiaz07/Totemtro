using UnityEngine;
using System.Collections;

public class ExploderAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;

    [Header("Explosion")]
    public float explosionRadius = 2f;
    public float damageOnDeath = 10f;
    public float damageOnProximity = 30f;

    [Header("Trigger")]
    public float triggerRange = 1.5f;
    public float explosionDelay = 1f;

    public GameObject explosionFX;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;
    SpriteRenderer sprite;

    bool isExploding = false;
    bool triggeredByProximity = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || isExploding) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= triggerRange)
        {
            StartExplosion(true);
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (enemy.IsKnocked()) return;
        if (isExploding) return;

        Vector2 dir =
            (player.position - transform.position).normalized;

        rb.linearVelocity = dir * speed;
    }

    // =====================================================
    // EXPLOSION TRIGGER
    // =====================================================

    public void StartExplosion(bool proximity)
    {
        if (isExploding) return;

        isExploding = true;
        triggeredByProximity = proximity;

        rb.linearVelocity = Vector2.zero;

        StartCoroutine(ExplosionCountdown());
    }

    IEnumerator ExplosionCountdown()
    {
        float timer = 0f;

        Color green = new Color32(124, 255, 0, 255);
        Color white = Color.white;

        while (timer < explosionDelay)
        {
            float t = timer / explosionDelay;

            // velocidad del parpadeo (cada vez más rápido)
            float speed = Mathf.Lerp(2f, 12f, t);

            float pingPong = Mathf.PingPong(Time.time * speed, 1f);

            if (sprite != null)
                sprite.color = Color.Lerp(green, white, pingPong);

            timer += Time.deltaTime;
            yield return null;
        }

        Explode();
    }

    // =====================================================
    // EXPLODE
    // =====================================================

    void Explode()
    {
        if (!gameObject.scene.isLoaded)
            return;

        float damage = triggeredByProximity
            ? damageOnProximity
            : damageOnDeath;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(
                    damage,
                    (hit.transform.position - transform.position).normalized
                );
            }
        }

        if (explosionFX != null)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
        }

        CameraShake.ShakeCamera(0.2f, 0.2f);

        GetComponent<ExploderAI>()?.Die();
    }

    // =====================================================
    // MUERTE NORMAL
    // =====================================================

    public void Die()
    {
        if (isExploding)
            return;

        // explosión débil si muere sin trigger
        triggeredByProximity = false;

        Explode();
    }
}