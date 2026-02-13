using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    public Weapon weapon;

    public SpriteRenderer lineSprite;
    public SpriteRenderer coneSprite;

    public float startOffset = 0.1f;

    void Awake()
    {
        if (weapon == null)
            weapon = GetComponentInParent<Weapon>();
    }

    void Update()
    {
        if (weapon == null ||
            weapon.currentWeapon == null ||
            !weapon.isAiming ||
            weapon.isAttacking)
        {
            DisableAll();
            return;
        }

        UpdateDirection();
        UpdateByWeapon();
    }

    void DisableAll()
    {
        lineSprite.enabled = false;
        coneSprite.enabled = false;
    }

    void UpdateDirection()
    {
        Vector2 dir =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.parent.position);

        if (dir.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Offset inicial del aim (radio 0.1)
        transform.localPosition = dir.normalized * startOffset;
    }

    void UpdateByWeapon()
    {
        if (weapon.currentWeapon.weaponType == WeaponType.ConeProjectile)
        {
            int step = weapon.GetVexStep();

            switch (step)
            {
                case 0:
                    ShowLine(weapon.currentWeapon.range * 0.16f);
                    break;

                case 1:
                    ShowCone(20f, weapon.currentWeapon.range * 0.16f);
                    break;

                case 2:
                    ShowCone(40f, weapon.currentWeapon.range * 0.16f);
                    break;
            }
        }
        else if (weapon.currentWeapon.weaponType == WeaponType.Projectile)
        {
            ShowLine(weapon.currentWeapon.range);
        }
        else
        {
            DisableAll();
        }
    }

    void ShowLine(float length)
    {
        lineSprite.enabled = true;
        coneSprite.enabled = false;

        lineSprite.color = new Color(1f, 1f, 0.2f, 0.18f);

        lineSprite.transform.localScale =
            new Vector3(length, 0.12f, 1f);
    }

    void ShowCone(float angle, float length)
    {
        lineSprite.enabled = false;
        coneSprite.enabled = true;

        float width =
            Mathf.Tan(angle * Mathf.Deg2Rad / 2f) * length * 2f;

        coneSprite.color = new Color(1f, 0.9f, 0.2f, 0.18f);

        coneSprite.transform.localScale =
            new Vector3(length, width, 1f);
    }
}
