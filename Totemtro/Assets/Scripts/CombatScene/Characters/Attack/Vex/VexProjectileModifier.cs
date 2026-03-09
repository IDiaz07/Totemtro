using UnityEngine;
using System.Collections;

public class VexProjectileModifier : MonoBehaviour
{
    public VexCardType cardType = VexCardType.None;

    PlayerStats stats;
    VexAttack vex;
    Projectile projectile;

    bool effectTriggered = false;

    [Header("Fire Effect")]
    public GameObject burnEffectPrefab;

    void Awake()
    {
        projectile = GetComponent<Projectile>();
        stats = FindObjectOfType<PlayerStats>();
        vex = FindObjectOfType<VexAttack>();
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
        if (effectTriggered) return;
        effectTriggered = true;

        // ✔ SOLO los disparos normales cargan la barra
        if (cardType == VexCardType.None)
            vex?.RegisterHit();

        switch (cardType)
        {
            case VexCardType.Star:
                SpawnStar(hitPos, enemy);
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
    void SpawnStar(Vector2 origin, Enemy originalEnemy)
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
                vex.starProjectilePrefab,
                origin,
                Quaternion.identity
            );

            Projectile p = proj.GetComponent<Projectile>();
            VexProjectileModifier mod = proj.GetComponent<VexProjectileModifier>();

            if (p != null)
            {
                p.Initialize(
                    8f,
                    12f,      // velocidad fija estrella
                    6f,       // 🔥 radio real
                    dir,
                    Vector2.zero,
                    false
                );
            }

            if (mod != null)
                mod.cardType = VexCardType.None;

            // ignorar enemigo inicial
            Collider2D starCol = proj.GetComponent<Collider2D>();
            Collider2D enemyCol = originalEnemy.GetComponent<Collider2D>();

            if (starCol && enemyCol)
                Physics2D.IgnoreCollision(starCol, enemyCol);
        }
    }

    IEnumerator TempIgnore(Collider2D a, Collider2D b)
    {
        Physics2D.IgnoreCollision(a, b, true);

        yield return new WaitForSeconds(0.12f);

        Physics2D.IgnoreCollision(a, b, false);
    }

    // 🔥 FIRE
    void ApplyBurn(Enemy enemy)
    {
        BurnEffect burn = enemy.GetComponent<BurnEffect>();

        if (burn != null) return;

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