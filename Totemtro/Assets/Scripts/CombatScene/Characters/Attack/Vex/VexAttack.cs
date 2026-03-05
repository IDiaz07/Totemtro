using UnityEngine;

public enum VexCardType
{
    None,
    Skull,
    Star,
    Fire,
    Fang,
    Spade
}

public class VexAttack : MonoBehaviour
{
    [Header("Random Bar")]
    public int maxPoints = 5;
    int currentPoints = 0;

    [Header("Projectiles")]
    public GameObject defaultProjectilePrefab;
    public GameObject skullProjectilePrefab;
    public GameObject starProjectilePrefab;
    public GameObject fireProjectilePrefab;
    public GameObject fangProjectilePrefab;
    public GameObject spadeProjectilePrefab;

    [Header("Card State")]
    public bool hasCard = false;
    public VexCardType currentCard = VexCardType.None;

    Weapon weapon;
    PlayerStats playerStats;

    void Awake()
    {
        weapon = GetComponent<Weapon>();
        playerStats = GetComponentInParent<PlayerStats>();
    }

    // =====================================================
    // 🔫 DISPARO
    // =====================================================

    public void ShootVex()
    {
        if (weapon.firePoint == null) return;

        Vector2 direction =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - weapon.firePoint.position).normalized;

        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        Vector2 inheritedVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

        float damage = playerStats.Damage;
        GameObject projectilePrefab = defaultProjectilePrefab;
        VexCardType cardTypeToAssign = VexCardType.None;

        if (hasCard)
        {
            cardTypeToAssign = currentCard;

            switch (currentCard)
            {
                case VexCardType.Skull:
                    projectilePrefab = skullProjectilePrefab;
                    break;

                case VexCardType.Star:
                    projectilePrefab = starProjectilePrefab;
                    break;

                case VexCardType.Fire:
                    projectilePrefab = fireProjectilePrefab;
                    break;

                case VexCardType.Fang:
                    projectilePrefab = fangProjectilePrefab;
                    break;

                case VexCardType.Spade:
                    projectilePrefab = spadeProjectilePrefab;
                    damage *= 3f;
                    break;
            }
        }

        GameObject proj = Instantiate(
            projectilePrefab,
            weapon.firePoint.position,
            Quaternion.identity
        );

        Projectile projectile = proj.GetComponent<Projectile>();
        VexProjectileModifier modifier = proj.GetComponent<VexProjectileModifier>();

        if (projectile != null)
        {
            projectile.Initialize(
                damage,
                playerStats.ProjectileSpeed,
                weapon.currentWeapon.range,
                direction,
                inheritedVelocity * 0.4f,
                false,
                playerStats.Pierce,
                playerStats.Ricochet
            );
        }

        if (modifier != null)
        {
            modifier.cardType = cardTypeToAssign;
        }

        // consumir carta
        hasCard = false;
        currentCard = VexCardType.None;
    }

    // =====================================================
    // 📈 BARRA
    // =====================================================

    public void RegisterHit()
    {
        currentPoints++;

        if (currentPoints >= maxPoints)
        {
            GenerateRandomCard();
            currentPoints = 0;
        }
    }

    void GenerateRandomCard()
    {
        // 🔥 CORRECCIÓN IMPORTANTE
        currentCard = (VexCardType)Random.Range(
            (int)VexCardType.Skull,
            (int)VexCardType.Spade + 1
        );

        hasCard = true;

        Debug.Log("🎴 Nueva carta: " + currentCard);
    }

    public float GetBarPercent()
    {
        return (float)currentPoints / maxPoints;
    }
}