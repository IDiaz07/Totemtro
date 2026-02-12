using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public WeaponData currentWeapon;
    public SpriteRenderer spriteRenderer;

    float lastAttackTime;

    public void SetWeapon(WeaponData data)
    {
        currentWeapon = data;
        spriteRenderer.sprite = data.weaponSprite;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= lastAttackTime + 1f / currentWeapon.fireRate)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Projectile:
                ShootProjectile();
                break;

            case WeaponType.MeleeArc:
                DoMeleeAttack();
                break;
        }
    }

    void ShootProjectile()
    {
        GameObject proj = Instantiate(
            currentWeapon.projectilePrefab,
            transform.position,
            transform.rotation
        );

        proj.GetComponent<Projectile>()
            .Initialize(currentWeapon.damage, currentWeapon.projectileSpeed, currentWeapon.range);
    }

    void DoMeleeAttack()
    {
        float radius = currentWeapon.meleeRadius;
        float angle = currentWeapon.meleeAngle;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float enemyAngle = Vector2.Angle(transform.right, dirToEnemy);

            if (enemyAngle <= angle / 2f)
            {
                hit.GetComponent<Enemy>().TakeDamage(currentWeapon.damage);
            }
        }

        StartCoroutine(SwingAnimation());
    }

    IEnumerator SwingAnimation()
    {
        float duration = 0.2f;
        float timer = 0f;

        float start = -60f;
        float end = 60f;

        while (timer < duration)
        {
            float t = timer / duration;
            float rot = Mathf.Lerp(start, end, t);

            transform.localRotation = Quaternion.Euler(0, 0, rot);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
    }
}
