using UnityEngine;

public class NyraAttack : MonoBehaviour
{
    PlayerStats playerStats;

    void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();
    }

    public void Execute(WeaponData weaponData, Transform firePoint)
    {
        if (weaponData == null) return;
        if (weaponData.projectilePrefab == null) return;

        Vector2 direction =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - firePoint.position).normalized;

        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        Vector2 inheritedVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

        GameObject proj = Instantiate(
            weaponData.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null) return;

        float damage = playerStats != null ? playerStats.Damage : weaponData.damage;
        float speed = playerStats != null ? playerStats.ProjectileSpeed : weaponData.projectileSpeed;
        int pierce = playerStats != null ? playerStats.Pierce : 0;
        int ricochet = playerStats != null ? playerStats.Ricochet : 0;

        projectile.Initialize(
            damage,
            speed,
            weaponData.range,
            direction,
            inheritedVelocity * 0.4f,
            false,
            pierce,
            ricochet
        );
    }
}
