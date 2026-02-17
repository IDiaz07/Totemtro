using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Damage { get; private set; }
    float speed;
    float range;
    bool isCritical;
    int pierce;
    int ricochet;
    int enemiesHit = 0;


    Vector2 direction;
    Vector2 inheritedVelocity;
    Vector3 startPos;

    // 🔹 VERSION ORIGINAL (NO LA BORRES)
    public void Initialize(
        float dmg,
        float spd,
        float rng,
        Vector2 dir,
        Vector2 inheritedVel,
        bool crit
    )
    {
        Damage = dmg;
        speed = spd;
        range = rng;
        direction = dir.normalized;
        inheritedVelocity = inheritedVel;
        isCritical = crit;

        startPos = transform.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 🔹 VERSION NUEVA (con tótems)
    public void Initialize(
        float dmg,
        float spd,
        float rng,
        Vector2 dir,
        Vector2 inheritedVel,
        bool crit,
        int pierceCount,
        int ricochetCount
    )
    {
        Initialize(dmg, spd, rng, dir, inheritedVel, crit);

        pierce = pierceCount;
        ricochet = ricochetCount;
    }



    void Update()
    {
        transform.position +=
            (Vector3)((direction * speed + inheritedVelocity)
            * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 🔹 Golpea Enemy clásico
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(Damage, direction, isCritical);
                enemiesHit++;

                if (enemiesHit > pierce)
                    Destroy(gameObject);

                return;
            }
        }

        // 🔹 Golpea NullGuardian
        NullGuardian guardian = other.GetComponent<NullGuardian>();

        if (guardian != null)
        {
            guardian.TakeDamage(Damage);

            enemiesHit++;

            if (enemiesHit > pierce)
                Destroy(gameObject);
        }
    }
}
