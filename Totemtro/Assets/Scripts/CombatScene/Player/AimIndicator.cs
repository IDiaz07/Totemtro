using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    public Weapon weapon;

    [Header("Sprites")]
    public SpriteRenderer lineSprite;
    public SpriteRenderer coneSprite;
    public SpriteRenderer circleSprite;

    public float startOffset = 0.1f;

    void Awake()
    {
        if (weapon == null)
            weapon = GetComponentInParent<Weapon>();
    }

    void Update()
    {
        bool dashAiming =
            weapon.currentWeapon.weaponType == WeaponType.KaelBlade &&
            Input.GetMouseButton(1);

        bool shotgunAiming =
            weapon.currentWeapon.weaponType == WeaponType.MurrayAnchor &&
            Input.GetMouseButton(1);

        if (weapon == null ||
            weapon.currentWeapon == null ||
            (!weapon.isAiming && !dashAiming && !shotgunAiming) ||
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
        if (lineSprite != null) lineSprite.enabled = false;
        if (coneSprite != null) coneSprite.enabled = false;
        if (circleSprite != null) circleSprite.enabled = false;
    }

    void UpdateDirection()
    {
        Vector2 dir =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.parent.position);

        if (dir.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (weapon.currentWeapon.weaponType == WeaponType.KaelBlade)
            transform.localPosition = Vector3.zero;
        else
            transform.localPosition = dir.normalized * startOffset;
    }

    void UpdateByWeapon()
    {
        var type = weapon.currentWeapon.weaponType;

        if (type == WeaponType.GrimRuneBurst)
        {
            ShowCone(
                weapon.currentWeapon.meleeAngle,
                weapon.currentWeapon.meleeRadius
            );
        }

        else if (type == WeaponType.MurrayAnchor)
        {
            if (Input.GetMouseButton(1))
            {
                // escopeta
                ShowCone(80f, 0.5f);
            }
            else
            {
                // ancla
                ShowCone(
                    weapon.currentWeapon.murrayConeAngle,
                    weapon.currentWeapon.murrayRadius * 1.4f
                );
            }
        }

        else if (type == WeaponType.OrinBurst ||
                 type == WeaponType.Projectile ||
                 type == WeaponType.NyraBloodOrb)
        {
            ShowLine(weapon.currentWeapon.range * 0.14f, 0.12f);
        }

        // VEX
        else if (type == WeaponType.VexProyectile)
        {
            ShowLine(weapon.currentWeapon.range * 0.14f, 0.24f);
        }

        // KAEL
        else if (type == WeaponType.KaelBlade)
        {
            if (Input.GetMouseButton(1))
            {
                // Dash aim
                ShowLine(1.2f, 1f);
            }
            else
            {
                // Circular attack
                ShowCircle(1.6f);
            }
        }

        // SELENEs
        else if (type == WeaponType.SeleneProjectile)
        {
            ShowLine(weapon.currentWeapon.range * 0.14f, 0.48f);
        }

        else
        {
            DisableAll();
        }
    }

    void ShowLine(float length, float width)
    {
        if (lineSprite == null) return;

        lineSprite.enabled = true;
        if (coneSprite != null) coneSprite.enabled = false;
        if (circleSprite != null) circleSprite.enabled = false;

        lineSprite.color = new Color(1f, 1f, 0.2f, 0.18f);

        lineSprite.transform.localScale =
            new Vector3(length, width, 1f);
    }

    void ShowCone(float angle, float length)
    {
        if (coneSprite == null) return;

        if (lineSprite != null) lineSprite.enabled = false;
        coneSprite.enabled = true;
        if (circleSprite != null) circleSprite.enabled = false;

        float width =
            Mathf.Tan(angle * Mathf.Deg2Rad / 2f) * length * 2f;

        coneSprite.color = new Color(1f, 0.9f, 0.2f, 0.18f);

        coneSprite.transform.localScale =
            new Vector3(length, width, 1f);
    }

    void ShowCircle(float radius)
    {
        if (circleSprite == null) return;

        if (lineSprite != null) lineSprite.enabled = false;
        if (coneSprite != null) coneSprite.enabled = false;

        circleSprite.enabled = true;

        circleSprite.color = new Color(1f, 0.9f, 0.2f, 0.15f);

        circleSprite.transform.localScale =
            new Vector3(radius, radius, 1f);
    }
}