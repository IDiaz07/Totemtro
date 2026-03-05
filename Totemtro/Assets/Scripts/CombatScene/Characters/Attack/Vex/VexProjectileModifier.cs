using UnityEngine;
using System.Collections;

public class VexProjectileModifier : MonoBehaviour
{
    public VexCardType cardType = VexCardType.None;

    PlayerStats stats;
    VexAttack vex;
    Projectile projectile;

    [Header("Fire Effect")]
    public GameObject burnEffectPrefab;

    void Awake()
    {
        stats = FindObjectOfType<PlayerStats>();
        vex = FindObjectOfType<VexAttack>();
        projectile = GetComponent<Projectile>();
    }

    void OnEnable()
    {
        if (projectile != null)
            projectile.OnEnemyHit += ApplyEffect;
    }

    void OnDisable()
    {
        if (projectile != null)
            projectile.OnEnemyHit -= ApplyEffect;
    }

    void ApplyEffect(Enemy enemy, Vector2 hitPos)
    {
        vex?.RegisterHit();

        switch (cardType)
        {
            case VexCardType.Star:
                SpawnStar(hitPos);
                break;

            case VexCardType.Fire:
                ApplyBurn(enemy);
                break;

            case VexCardType.Fang:
                HealPlayer();
                break;

            case VexCardType.Skull:
                Explode(hitPos);
                break;
        }
    }

    // ⭐ STAR
    void SpawnStar(Vector2 origin)
    {
        int count = 5;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;

            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;

            GameObject proj = Instantiate(
                vex.starProjectilePrefab,   // IMPORTANTE: usar el prefab de star
                origin,
                Quaternion.identity
            );

            Projectile p = proj.GetComponent<Projectile>();

            if (p != null)
            {
                p.Initialize(
                    8f,
                    stats.ProjectileSpeed,
                    6f,
                    dir,
                    Vector2.zero,
                    false
                );
            }
        }
    }

    // 🔥 FIRE
    void ApplyBurn(Enemy enemy)
    {
        // evitar stack de burn
        BurnEffect burn = enemy.GetComponent<BurnEffect>();
        if (burn != null)
            return;

        burn = enemy.gameObject.AddComponent<BurnEffect>();
        burn.Initialize(enemy, burnEffectPrefab);
    }

    // 🩸 FANG
    void HealPlayer()
    {
        PlayerHealth health = FindObjectOfType<PlayerHealth>();
        health?.Heal(10f);
    }

    // 💀 SKULL
    void Explode(Vector2 position)
    {
        float radius = 2f;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(position, radius);

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();

            if (e != null)
            {
                Vector2 dir =
                    ((Vector2)e.transform.position - position).normalized;

                e.TakeDamage(20f, dir, false);
            }
        }
    }
}