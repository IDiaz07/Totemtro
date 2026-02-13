using UnityEngine;

public class Projectile : MonoBehaviour
{
    float damage;
    float speed;
    float range;

    Vector2 direction;
    Vector2 inheritedVelocity;   // ← Velocidad heredada del jugador
    Vector3 startPos;

    public void Initialize(
        float dmg,
        float spd,
        float rng,
        Vector2 dir,
        Vector2 playerVelocity   // ← Nueva variable recibida
    )
    {
        damage = dmg;
        speed = spd;
        range = rng;

        direction = dir.normalized;
        inheritedVelocity = playerVelocity;

        startPos = transform.position;

        // Rotar el proyectil hacia la dirección
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // Movimiento natural con herencia de velocidad
        Vector2 finalVelocity = direction * speed + inheritedVelocity;

        transform.position += (Vector3)(finalVelocity * Time.deltaTime);

        // Destruir si supera el rango
        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
