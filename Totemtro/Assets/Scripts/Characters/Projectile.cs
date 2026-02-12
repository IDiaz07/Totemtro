using UnityEngine;

public class Projectile : MonoBehaviour
{
    float damage;
    float speed;
    float range;

    Vector2 direction;
    Vector3 startPos;

    public void Initialize(float dmg, float spd, float rng, Vector2 dir)
    {
        damage = dmg;
        speed = spd;
        range = rng;

        direction = dir.normalized;
        startPos = transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
