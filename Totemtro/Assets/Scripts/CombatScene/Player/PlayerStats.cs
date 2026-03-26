using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    HeroController hero;
    Weapon weapon;
    PlayerMovement movement;
    PlayerHealth health;
    TotemInventory inventory;

    // =========================
    // BASE
    // =========================
    float baseDamage;
    float baseFireRate;
    float baseProjectileSpeed;
    float baseMoveSpeed;
    float baseMaxHealth;

    // =========================
    // FINAL
    // =========================
    public float Damage
    {
        get
        {
            float final = _damage;

            // 🩸 BLOOD PRICE DINÁMICO
            if (hasBloodPrice && health != null)
            {
                float missing =
                    1f - health.GetCurrentHealthPercent();

                final *= 1f + (missing * bloodPriceMaxBonus);
            }

            return final;
        }
    }

    float _damage;

    public float FireRate { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public float MoveSpeed { get; private set; }
    public float MaxHealth { get; private set; }

    public int ExtraProjectiles { get; private set; }
    public int Pierce { get; private set; }
    public int Ricochet { get; private set; }

    public bool hasBloodPrice = false;
    public float bloodPriceMaxBonus = 0.6f;

    void Awake()
    {
        hero = GetComponent<HeroController>();
        weapon = GetComponentInChildren<Weapon>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        inventory = GetComponent<TotemInventory>();
    }

    // =========================
    // INIT BASE
    // =========================

    public void Initialize()
    {
        if (weapon == null || weapon.currentWeapon == null)
            return;

        baseDamage = weapon.currentWeapon.damage;
        baseFireRate = weapon.currentWeapon.fireRate;
        baseProjectileSpeed = weapon.currentWeapon.projectileSpeed;

        baseMoveSpeed = hero.currentHero.moveSpeed;
        baseMaxHealth = hero.currentHero.maxHealth;

        Recalculate();
    }

    // =========================
    // RECALCULATE
    // =========================

    public void Recalculate()
    {
        // RESET BASE
        _damage = baseDamage;
        FireRate = baseFireRate;
        ProjectileSpeed = baseProjectileSpeed;
        MoveSpeed = baseMoveSpeed;
        MaxHealth = baseMaxHealth;

        ExtraProjectiles = 0;
        Pierce = 0;
        Ricochet = 0;

        hasBloodPrice = false;

        // RESET DEFENSIVE
        health.dodgeChance = 0f;
        health.damageReductionPercent = 0f;
        health.healthRegenPerSecond = 0f;
        health.shieldAmount = 0f;
        health.hasSecondWind = false;

        movement.hasDash = false;

        if (inventory == null) return;

        foreach (var totem in inventory.ownedTotems)
        {
            ApplyTotem(totem);
        }

        ApplyEquipmentArmor();

        ApplyToCharacter();
    }

    // =========================
    // APPLY TOTEM
    // =========================

    void ApplyTotem(TotemData data)
    {
        float rarityMultiplier = GetRarityMultiplier(data.rarity);

        switch (data.totemType)
        {
            case TotemType.Power:
                _damage *= 1f + (0.2f * rarityMultiplier);
                break;

            case TotemType.Swiftness:
                MoveSpeed *= 1f + (0.15f * rarityMultiplier);
                break;

            case TotemType.Vitality:
                MaxHealth *= 1f + (0.2f * rarityMultiplier);
                break;

            case TotemType.RapidBullets:
                FireRate *= 1f + (0.2f * rarityMultiplier);
                ProjectileSpeed *= 1f + (0.15f * rarityMultiplier);
                break;

            case TotemType.DualFire:
                ApplyDualFire(rarityMultiplier);
                break;

            case TotemType.Piercing:
                Pierce += Mathf.RoundToInt(1f * rarityMultiplier);
                break;

            case TotemType.Ricochet:
                Ricochet += Mathf.RoundToInt(1f * rarityMultiplier);
                break;

            case TotemType.Evasion:
                health.dodgeChance += 0.1f * rarityMultiplier;
                break;

            case TotemType.Fortitude:
                health.damageReductionPercent += 0.1f * rarityMultiplier;
                break;

            case TotemType.Recovery:
                health.healthRegenPerSecond += 1f * rarityMultiplier;
                break;

            case TotemType.Shielding:
                health.shieldAmount += 20f * rarityMultiplier;
                break;

            case TotemType.SecondWind:
                health.hasSecondWind = true;
                break;

            case TotemType.Dash:
                movement.hasDash = true;
                movement.dashForce += 3f * rarityMultiplier;
                break;

            case TotemType.BloodPrice:
                hasBloodPrice = true;
                bloodPriceMaxBonus += 0.2f * rarityMultiplier;
                break;

            case TotemType.Retaliation:
                health.retaliationMultiplier += 0.25f * rarityMultiplier;
                break;
        }
    }

    float GetRarityMultiplier(TotemRarity rarity)
    {
        switch (rarity)
        {
            case TotemRarity.Common: return 1f;
            case TotemRarity.Rare: return 1.5f;
            case TotemRarity.Legendary: return 2.5f;
        }

        return 1f;
    }

    public float dualFireChance = 0f;

    void ApplyDualFire(float rarityMultiplier)
    {
        dualFireChance += 0.3f * rarityMultiplier;
    }


    // =========================
    // APPLY TO CHARACTER
    // =========================

    void ApplyToCharacter()
    {
        if (movement != null)
            movement.speed = MoveSpeed * speedMultiplier;

        if (hero != null)
            hero.SetMaxHealth(MaxHealth);
    }

    // =========================
    // TEMP MULTIPLIERS
    // =========================

    float speedMultiplier = 1f;

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        ApplyToCharacter();
    }

    void ApplyEquipmentArmor()
    {
        if (EquipmentSystem.Instance == null) return;

        foreach (var slot in EquipmentSystem.Instance.equipmentSlots)
        {
            if (slot.item == null) continue;

            ItemData item = slot.item;

            if (item.itemType != ItemType.Equipment) continue;

            health.damageReductionPercent += item.damageReduction;
        }

        // CAP opcional (recomendado)
        health.damageReductionPercent =
            Mathf.Clamp(health.damageReductionPercent, 0f, 0.75f);
    }
}
