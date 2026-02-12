using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    public Weapon weapon;

    public SpriteRenderer lineSprite;
    public SpriteRenderer coneSprite;
    public SpriteRenderer auraSprite;
    public SpriteRenderer sniperDot;
    public SpriteRenderer sniperLine;


    public float pistolLength = 2f;
    public float sniperLength = 6f;

    public ParticleSystem runeParticles;

    void Update()
    {
        if (weapon.currentWeapon == null || weapon.isAttacking)
        {
            SetAll(false);
            return;
        }

        UpdateDirection();
        UpdateByWeaponType();
    }

    void SetAll(bool state)
    {
        lineSprite.enabled = state;
        coneSprite.enabled = false;
        auraSprite.enabled = false;
        sniperDot.enabled = false;
        sniperLine.enabled = false;

    }

    void UpdateDirection()
    {
        Vector2 dir =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.parent.position);

        if (dir.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UpdateByWeaponType()
    {
        switch (weapon.currentWeapon.weaponType)
        {
            case WeaponType.Projectile:

                if (weapon.currentWeapon.aimLength > 5f)
                    ShowSniper(weapon.currentWeapon.aimLength);
                else
                    ShowLine(weapon.currentWeapon.aimLength);

                break;


            case WeaponType.ConeProjectile:
                UpdateVexCone();
                break;

            case WeaponType.GrimRuneBurst:
                ShowCone(40f, 3f);
                break;

            default:
                DisableGrimParticles();
                break;
        }
    }

    void ShowLine(float length)
    {
        lineSprite.enabled = true;
        coneSprite.enabled = false;
        auraSprite.enabled = false;

        lineSprite.color = new Color(1f, 1f, 0.2f, 0.2f);
        transform.localScale = new Vector3(length, 0.15f, 1f);
    }

    void UpdateVexCone()
    {
        int step = weapon.GetVexStep();
        float glowIntensity = 0.15f + (step * 0.08f);

        coneSprite.color = new Color(1f, 0.85f, 0.2f, glowIntensity);
        auraSprite.color = new Color(1f, 0.4f, 0.1f, glowIntensity * 0.6f);

        lineSprite.enabled = false;
        coneSprite.enabled = true;
        auraSprite.enabled = true;

        if (step == 1)
            ShowLine(2.5f);
        else if (step == 2)
            ShowCone(18f, 2.8f);
        else if (step == 3)
            ShowCone(35f, 3.5f);
    }

    void ShowCone(float angle, float length)
    {
        float width = Mathf.Tan(angle * Mathf.Deg2Rad / 2f) * length * 2f;
        transform.localScale = new Vector3(length, width, 1f);

        coneSprite.color = new Color(1f, 0.9f, 0.2f, 0.22f);
        auraSprite.color = new Color(0.8f, 0.2f, 0.2f, 0.12f);
    }

    void ShowSniper(float length)
    {
        lineSprite.enabled = true;
        coneSprite.enabled = false;
        auraSprite.enabled = false;

        lineSprite.color = new Color(1f, 1f, 0.6f, 0.15f);
        transform.localScale = new Vector3(length, 0.05f, 1f);

        // Punto final brillante
        sniperDot.enabled = true;
        sniperDot.transform.localPosition = new Vector3(length, 0f, 0f);

        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.1f;
        sniperDot.transform.localScale = Vector3.one * pulse;
    }

    void ShowGrimCone(float angle, float length)
    {
        ShowCone(angle, length);

        runeParticles.gameObject.SetActive(true);

        var emission = runeParticles.emission;
        emission.rateOverTime = 15;
    }

    void DisableGrimParticles()
    {
        if (runeParticles == null) return;

        if (runeParticles.isPlaying)
            runeParticles.Stop();

        runeParticles.gameObject.SetActive(false);
    }

}
