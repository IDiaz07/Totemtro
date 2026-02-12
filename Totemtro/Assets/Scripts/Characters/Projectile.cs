using UnityEngine;

public class Projectile : MonoBehaviour
{
    float damage;
    float speed;
    float range;
    Vector3 startPos;

    public void Initialize(float dmg, float spd, float rng)
    {
        damage = dmg;
        speed = spd;
        range = rng;
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

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
