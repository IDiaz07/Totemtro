using UnityEngine;

public class SeleneAttack : MonoBehaviour
{
    Weapon weapon;
    PlayerStats stats;

    public GameObject fallingOrbPrefab;
    public GameObject chargeOrbPrefab;

    GameObject currentChargeOrb;

    void Awake()
    {
        weapon = GetComponent<Weapon>();
        stats = GetComponentInParent<PlayerStats>();
    }

    void Update()
    {
        // empezar carga
        if (Input.GetMouseButtonDown(0))
        {
            StartCharge();
        }

        // soltar disparo
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseCharge();
        }
    }

    void StartCharge()
    {
        if (chargeOrbPrefab == null) return;

        Vector2 pos = weapon.firePoint.position;

        currentChargeOrb = Instantiate(
            chargeOrbPrefab,
            pos,
            Quaternion.identity,
            weapon.firePoint
        );
    }

    void ReleaseCharge()
    {
        if (currentChargeOrb != null)
            Destroy(currentChargeOrb);

        NormalAttack();
    }

    // =================================
    // ATAQUE PRINCIPAL
    // =================================

    public void NormalAttack()
    {
        if (weapon.firePoint == null) return;
        if (weapon.currentWeapon.projectilePrefab == null) return;

        Vector2 dir =
        (Camera.main.ScreenToWorldPoint(Input.mousePosition)
        - weapon.firePoint.position).normalized;

        Vector2 spawnPos =
            (Vector2)weapon.firePoint.position + dir * 0.35f;

        GameObject proj = Instantiate(
            weapon.currentWeapon.projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        IgnorePlayerCollision(proj);

        SeleneProjectile projectile = proj.GetComponent<SeleneProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                stats.Damage,
                stats.ProjectileSpeed,
                weapon.currentWeapon.range,
                dir
            );
        }
    }

    // =================================
    // ATAQUE SECUNDARIO
    // =================================

    public void SecondaryAttack()
    {
        if (fallingOrbPrefab == null) return;

        Vector2 pos = transform.parent.position;

        Instantiate(
            fallingOrbPrefab,
            pos,
            Quaternion.identity
        );
    }

    void IgnorePlayerCollision(GameObject proj)
    {
        Collider2D playerCollider = GetComponentInParent<Collider2D>();
        Collider2D projCollider = proj.GetComponent<Collider2D>();

        if (playerCollider != null && projCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, projCollider);
        }
    }
}