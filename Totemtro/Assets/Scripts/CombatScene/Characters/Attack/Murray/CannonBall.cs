using UnityEngine;

public class CannonBall : MonoBehaviour
{
    float damage;
    float speed;
    float knockback;

    Vector2 direction;

    float range;
    Vector3 startPos;

    public void Initialize(
        float dmg,
        Vector2 dir,
        float spd,
        float kb,
        float rng
    )
    {
        damage = dmg;
        direction = dir.normalized;
        speed = spd;
        knockback = kb;
        range = rng;

        startPos = transform.position;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // desaparecer por distancia
        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy enemy = col.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, direction, false);

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.AddForce(direction * knockback, ForceMode2D.Impulse);

            Destroy(gameObject);
            return;
        }

        if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}