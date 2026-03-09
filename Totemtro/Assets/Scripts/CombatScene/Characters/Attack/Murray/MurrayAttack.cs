using UnityEngine;

public class MurrayAttack : MonoBehaviour
{
    Weapon weapon;
    PlayerStats playerStats;

    [Header("Prefabs")]
    public GameObject anchorPrefab;
    public GameObject cannonBallPrefab;

    public Transform firePoint;

    void Start()
    {
        weapon = GetComponent<Weapon>();
        playerStats = GetComponentInParent<PlayerStats>();
    }

    // =================================
    // ⚓ LEFT CLICK – ANCHOR
    // =================================

    public void AnchorAttack()
    {
        if (weapon == null) return;
        if (anchorPrefab == null) return;

        WeaponData data = weapon.currentWeapon;

        Vector2 direction =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - firePoint.position).normalized;

        GameObject anchor = Instantiate(
            anchorPrefab,
            firePoint.position,
            Quaternion.identity
        );

        MurrayAnchor a = anchor.GetComponent<MurrayAnchor>();

        a.Initialize(
            transform.parent,
            direction,
            playerStats.Damage,
            data.range,
            data.murrayAnchorDamageRadius,
            data.murrayChainWidth
        );
    }

    // =================================
    // 💣 RIGHT CLICK – CANNON
    // =================================

    public int pelletCount = 6;
    public float spreadAngle = 25f;

    public ParticleSystem muzzleParticles;

    public void CannonShot()
    {
        Vector2 baseDir =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - firePoint.position).normalized;

        float totalSpread = spreadAngle;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle =
                -totalSpread / 2f +
                totalSpread * (i / (float)(pelletCount - 1));

            Vector2 dir = RotateVector(baseDir, angle);

            GameObject bullet =
                Instantiate(cannonBallPrefab, firePoint.position, Quaternion.identity);

            CannonBall ball = bullet.GetComponent<CannonBall>();

            ball.Initialize(
                playerStats.Damage * 0.6f,
                dir,
                10f,
                12f,
                weapon.currentWeapon.range
            );
        }

        // partículas
        if (muzzleParticles != null)
            muzzleParticles.Play();

        CameraShake.ShakeCamera(0.25f, 0.08f);
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;

        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}