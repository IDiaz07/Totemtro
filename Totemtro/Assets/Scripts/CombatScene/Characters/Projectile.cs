using UnityEngine;
using System.Collections.Generic;

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

    public System.Action<Enemy, Vector2> OnEnemyHit;

    HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    // enemigo a ignorar (para STAR)
    Enemy ignoredEnemy;

    // =====================================================
    // INITIALIZE
    // =====================================================

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

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        direction = dir.normalized;

        inheritedVelocity = inheritedVel;
        isCritical = crit;

        startPos = transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

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

    // =====================================================
    // IGNORE ENEMY (STAR FIX)
    // =====================================================

    public void IgnoreEnemy(Enemy enemy)
    {
        ignoredEnemy = enemy;
    }

    // =====================================================
    // MOVEMENT
    // =====================================================

    void Update()
    {
        Vector2 velocity = direction * speed + inheritedVelocity;

        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (Vector3.Distance(startPos, transform.position) >= range)
            Destroy(gameObject);
    }

    // =====================================================
    // COLLISION
    // =====================================================

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            // ignorar enemigo original (STAR)
            if (enemy == ignoredEnemy)
                return;

            // evitar hits múltiples
            if (hitEnemies.Contains(enemy))
                return;

            hitEnemies.Add(enemy);

            enemy.TakeDamage(Damage, direction, isCritical);

            OnEnemyHit?.Invoke(enemy, transform.position);

            enemiesHit++;

            if (enemiesHit > pierce)
                Destroy(gameObject);

            return;
        }

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