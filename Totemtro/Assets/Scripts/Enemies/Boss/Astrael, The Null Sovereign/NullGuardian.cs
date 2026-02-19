using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class NullGuardian : MonoBehaviour
{
    [Header("Stats")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 150f;
    public float launchSpeed = 12f;
    public float orbitDuration = 2f;
    public float damage = 12f;
    public float maxHealth = 1f;
    public float lifeTimeAfterLaunch = 1f;

    Transform boss;
    Transform player;
    Rigidbody2D rb;

    float angle;
    float currentHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public void Init(Transform bossTransform, Transform playerTransform)
    {
        boss = bossTransform;
        player = playerTransform;

        angle = Random.Range(0f, 360f);

        StartCoroutine(OrbitThenLaunch());
    }

    IEnumerator OrbitThenLaunch()
    {
        float timer = 0f;

        while (timer < orbitDuration)
        {
            if (boss == null)
            {
                Destroy(gameObject);
                yield break;
            }

            angle += orbitSpeed * Time.deltaTime;

            Vector2 offset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * orbitRadius;

            transform.position = (Vector2)boss.position + offset;

            timer += Time.deltaTime;
            yield return null;
        }

        if (player == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector2 dir =
            (player.position - transform.position).normalized;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = dir * launchSpeed;

        Destroy(gameObject, lifeTimeAfterLaunch);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 🔹 Golpea jugador
        if (col.CompareTag("Player"))
        {
            PlayerHealth playerHealth =
                col.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                Vector2 dir =
                    (col.transform.position - transform.position).normalized;

                playerHealth.TakeDamage(damage, dir);
            }

            Destroy(gameObject);
            return;
        }

        // 🔹 Choca con muro dinámico
        if (col.GetComponent<ArenaGuardian>() != null)
        {
            Destroy(gameObject);
            return;
        }

        // 🔹 Recibe daño de proyectil
        Projectile proj = col.GetComponent<Projectile>();
        if (proj != null)
        {
            TakeDamage(proj.Damage);
            Destroy(col.gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}
