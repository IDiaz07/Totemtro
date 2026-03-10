using UnityEngine;

public class MurrayAttack : MonoBehaviour
{
    Weapon weapon;
    PlayerStats playerStats;

    [Header("Prefabs")]
    public GameObject anchorPrefab;
    public GameObject cannonBallPrefab;

    [Header("Damage Multipliers")]
    public float anchorDamageMultiplier = 1.2f;
    public float chainDamageMultiplier = 0.8f;
    public float shotgunDamageMultiplier = 0.25f;

    [Header("Ranges")]
    public float anchorRange = 6f;
    public float shotgunRange = 7f;

    public Transform firePoint;

    [Header("Shotgun Cooldown")]
    public float shotgunCooldown = 1.2f;
    float lastShotgunTime = -999f;

    public float ShotgunCooldownRemaining { get; private set; }

    [Header("Shotgun Settings")]
    public int pelletCount = 8;
    public float spreadAngle = 22f;
    public float muzzleOffset = 0.15f;

    [Header("Shotgun Recoil")]
    public float recoilDistance = 0.08f;

    public ParticleSystem muzzleParticles;

    void Awake()
    {
        weapon = GetComponent<Weapon>();
        if (weapon == null)
            weapon = GetComponentInParent<Weapon>();

        if (weapon != null)
            playerStats = weapon.GetComponentInParent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("MurrayAttack: PlayerStats no encontrado");
        }
    }

    void Update()
    {
        if (ShotgunCooldownRemaining > 0f)
        {
            ShotgunCooldownRemaining -= Time.deltaTime;

            if (ShotgunCooldownRemaining < 0f)
                ShotgunCooldownRemaining = 0f;
        }
    }

    // =================================
    // ⚓ LEFT CLICK – ANCHOR
    // =================================

    public void AnchorAttack()
    {
        if (weapon == null || playerStats == null) return;
        if (anchorPrefab == null) return;

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
            playerStats.Damage * anchorDamageMultiplier,
            anchorRange,
            weapon.currentWeapon.murrayAnchorDamageRadius,
            weapon.currentWeapon.murrayChainWidth
        );
    }

    // =================================
    // 💣 RIGHT CLICK – SHOTGUN
    // =================================

    public void CannonShot()
    {
        Debug.Log("Shotgun fired");

        if (firePoint == null)
        {
            Debug.LogError("firePoint no asignado");
            return;
        }

        if (Time.time < lastShotgunTime + shotgunCooldown)
            return;

        if (firePoint == null)
        {
            Debug.LogError("MurrayAttack: firePoint no está asignado");
            return;
        }

        if (cannonBallPrefab == null)
        {
            Debug.LogError("MurrayAttack: cannonBallPrefab no está asignado");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError("MurrayAttack: playerStats no encontrado");
            return;
        }

        Vector2 baseDir =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - firePoint.position).normalized;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = Random.Range(-spreadAngle, spreadAngle);
            Vector2 dir = RotateVector(baseDir, angle);

            Vector3 spawnPos =
                firePoint.position +
                (Vector3)(dir * Random.Range(0f, muzzleOffset));

            GameObject bullet =
                Instantiate(cannonBallPrefab, spawnPos, Quaternion.identity);

            float rot =
                Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            bullet.transform.rotation =
                Quaternion.Euler(0, 0, rot);

            CannonBall ball = bullet.GetComponent<CannonBall>();

            if (ball == null)
            {
                Debug.LogError("El prefab de bala no tiene el script CannonBall");
                return;
            }

            float speed = Random.Range(10f, 12f);
            float knock = Random.Range(10f, 14f);

            ball.Initialize(
                playerStats.Damage * shotgunDamageMultiplier,
                dir,
                speed,
                knock,
                shotgunRange
            );
        }

        if (muzzleParticles != null)
            muzzleParticles.Play();

        lastShotgunTime = Time.time;
        ShotgunCooldownRemaining = shotgunCooldown;

        ApplyRecoil(baseDir);
    }

    // =================================
    // PLAYER RECOIL
    // =================================

    void ApplyRecoil(Vector2 shootDir)
    {
        Transform player = transform.parent;

        Vector3 recoil = -(Vector3)shootDir * recoilDistance;

        player.position += recoil;
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