using UnityEngine;

public class SeleneProjectile : MonoBehaviour
{
    float damage;
    float speed;
    float range;

    Vector2 direction;
    Vector2 startPos;

    bool hasImpacted = false;

    Rigidbody2D rb;

    [Header("Prefabs")]
    public GameObject poolPrefab;
    public GameObject healNumberPrefab;
    public GameObject impactVFX;

    [Header("Visual")]
    public float wobbleAmplitude = 0.08f;
    public float wobbleSpeed = 8f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float dmg, float spd, float rng, Vector2 dir)
    {
        damage = dmg;
        speed = spd;
        range = rng;

        direction = dir.normalized;
        startPos = transform.position;

        // rotar el sprite hacia la dirección
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    void Update()
    {
        if (hasImpacted) return;

        // wobble visual solo en escala
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmplitude;

        transform.localScale = Vector3.one * (1f + wobble);

        // rango máximo
        if (Vector2.Distance(startPos, transform.position) >= range)
        {
            Impact();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasImpacted) return;

        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, direction, false);
            Impact();
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.Heal(damage);
            SpawnHealNumber(player.transform.position, damage);
            Impact();
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Impact();
        }
    }

    void Impact()
    {
        hasImpacted = true;

        if (impactVFX != null)
            Instantiate(impactVFX, transform.position, Quaternion.identity);

        if (poolPrefab != null)
            Instantiate(poolPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void SpawnHealNumber(Vector3 pos, float amount)
    {
        if (healNumberPrefab == null) return;

        GameObject obj = Instantiate(
            healNumberPrefab,
            pos + Vector3.up * 0.6f,
            Quaternion.identity
        );

        HealNumber heal = obj.GetComponent<HealNumber>();

        if (heal != null)
            heal.SetHeal(amount);
    }
}