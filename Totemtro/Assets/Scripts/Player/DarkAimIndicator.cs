using UnityEngine;

public class DarkAimIndicator : MonoBehaviour
{
    public Weapon weapon;

    public SpriteRenderer cone;
    public SpriteRenderer aura;
    public ParticleSystem particles;

    float pulseTimer;

    void Update()
    {
        if (weapon.currentWeapon == null || weapon.isAttacking)
        {
            SetActive(false);
            return;
        }

        SetActive(true);

        UpdateDirection();
        UpdateShape();
        UpdateAuraPulse();
    }

    void SetActive(bool state)
    {
        cone.enabled = state;
        aura.enabled = state;

        if (state && !particles.isPlaying)
            particles.Play();
        else if (!state && particles.isPlaying)
            particles.Stop();
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

    void UpdateShape()
    {
        switch (weapon.currentWeapon.weaponType)
        {
            case WeaponType.ConeProjectile:
                UpdateVexVisual();
                break;

            case WeaponType.Projectile:
                SetLineVisual();
                break;
        }
    }

    void SetLineVisual()
    {
        transform.localScale = new Vector3(2.5f, 0.2f, 1f);
        cone.color = new Color(1f, 0.9f, 0.2f, 0.18f);
        aura.color = new Color(0.6f, 0.1f, 0.1f, 0.1f);
    }

    void UpdateVexVisual()
    {
        int step = weapon.GetVexStep();

        if (step == 1)
        {
            transform.localScale = new Vector3(2.2f, 0.2f, 1f);
            cone.color = new Color(1f, 0.9f, 0.2f, 0.18f);
        }
        else if (step == 2)
        {
            SetCone(18f, 2.8f, 0.22f);
        }
        else if (step == 3)
        {
            SetCone(35f, 3.5f, 0.3f);
            var emission = particles.emission;
            emission.rateOverTime = 25;
        }
    }

    void SetCone(float angle, float length, float alpha)
    {
        float width = Mathf.Tan(angle * Mathf.Deg2Rad / 2f) * length * 2f;
        transform.localScale = new Vector3(length, width, 1f);

        cone.color = new Color(1f, 0.85f, 0.15f, alpha);
        aura.color = new Color(0.8f, 0.15f, 0.15f, alpha * 0.6f);
    }

    void UpdateAuraPulse()
    {
        pulseTimer += Time.deltaTime * 3f;

        float pulse = 1f + Mathf.Sin(pulseTimer) * 0.05f;
        aura.transform.localScale = Vector3.one * pulse;
    }
}
