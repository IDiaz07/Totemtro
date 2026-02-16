using UnityEngine;

public class NyraBloodBall : MonoBehaviour
{
    public GameObject miniProjectilePrefab;
    public int miniCount = 6;
    public float miniDamageMultiplier = 0.6f;

    PlayerStats playerStats;

    void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        SpawnMiniProjectiles(other);
    }

    void SpawnMiniProjectiles(Collider2D originalEnemy)
    {
        float step = 360f / miniCount;

        for (int i = 0; i < miniCount; i++)
        {
            float angle = step * i;

            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject mini = Instantiate(
                miniProjectilePrefab,
                transform.position,
                Quaternion.identity
            );

            Projectile miniProj = mini.GetComponent<Projectile>();

            float damage = playerStats != null
    ? playerStats.Damage * miniDamageMultiplier
    : 5f;

            float speed = playerStats != null
                ? playerStats.ProjectileSpeed
                : 8f;

            int pierce = playerStats != null
                ? playerStats.Pierce
                : 0;

            int ricochet = playerStats != null
                ? playerStats.Ricochet
                : 0;

            miniProj.Initialize(
                damage,
                speed,
                4f,
                dir,
                Vector2.zero,
                false,
                pierce,
                ricochet
            );

            // 🔥 Ignorar el enemigo original
            Physics2D.IgnoreCollision(
                mini.GetComponent<Collider2D>(),
                originalEnemy
            );
        }
    }
}
