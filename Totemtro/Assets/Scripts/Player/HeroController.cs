using UnityEngine;
using System;

public class HeroController : MonoBehaviour
{
    public HeroData currentHero;
    public SpriteRenderer bodyRenderer;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public Action OnHealthChanged;

    PlayerMovement movement;
    Weapon weapon;

    int currentWeaponIndex = 0;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponentInChildren<Weapon>();
        GamePause.Reset();

        if (currentHero != null)
            ApplyHero();
    }


    // =========================================
    // APPLY HERO DATA
    // =========================================

    public void ApplyHero()
    {
        if (currentHero == null) return;

        MaxHealth = currentHero.maxHealth;
        CurrentHealth = MaxHealth;

        if (movement != null)
            movement.speed = currentHero.moveSpeed;

        if (bodyRenderer != null)
            bodyRenderer.sprite = currentHero.bodySprite;

        EquipWeapon(0);

        GetComponent<PlayerStats>()?.Initialize();

        OnHealthChanged?.Invoke();
    }

    // =========================================
    // HEALTH MANAGEMENT (AAA SAFE)
    // =========================================

    public void SetMaxHealth(float value)
    {
        float previousPercent = CurrentHealth / MaxHealth;

        MaxHealth = value;
        CurrentHealth = MaxHealth * previousPercent;

        OnHealthChanged?.Invoke();
    }

    public void SetCurrentHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0f, MaxHealth);
        OnHealthChanged?.Invoke();
    }

    public void ApplyDamage(float amount)
    {
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        if (CurrentHealth <= 0f)
            Die();

        OnHealthChanged?.Invoke();
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        OnHealthChanged?.Invoke();
    }

    public void IncreaseMaxHealth(float multiplier)
    {
        SetMaxHealth(MaxHealth * multiplier);
    }

    // =========================================
    // WEAPON
    // =========================================

    public void EquipWeapon(int index)
    {
        if (currentHero == null) return;
        if (currentHero.weapons == null) return;
        if (index < 0 || index >= currentHero.weapons.Count) return;
        if (weapon == null) return;

        currentWeaponIndex = index;
        weapon.SetWeapon(currentHero.weapons[index]);
    }

    // =========================================
    // DEATH
    // =========================================

    public void Die()
    {
        Debug.Log("Hero died");
        // aquí meteremos animación, restart, etc
    }
}
