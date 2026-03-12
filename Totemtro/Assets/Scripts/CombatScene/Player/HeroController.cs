using UnityEngine;
using System;
using System.Collections;

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

    public static Action OnPlayerDeath;
    bool isDead = false;

    float idleTimer = 0f;
    float idleThreshold = 3f;

    Vector2 lastLookDirection = Vector2.right;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponentInChildren<Weapon>();

        GamePause.Reset();

        // 🔥 Hero seleccionado desde el Hub
        if (HeroSelectionSystem.Instance != null &&
            HeroSelectionSystem.Instance.selectedHero != null)
        {
            currentHero = HeroSelectionSystem.Instance.selectedHero;
        }

        if (currentHero != null)
            ApplyHero();
    }

    // =========================================
    // APPLY HERO DATA
    // =========================================

    public void ApplyHero()
    {
        if (currentHero == null)
            return;

        MaxHealth = currentHero.maxHealth;
        CurrentHealth = MaxHealth;

        if (movement != null)
            movement.speed = currentHero.moveSpeed;

        // 🔥 Sprite inicial (idle mirando al frente)
        if (bodyRenderer != null &&
            currentHero.directionalSprites != null)
        {
            bodyRenderer.sprite = currentHero.directionalSprites.FrontView;
        }

        EquipWeapon(0);

        GetComponent<PlayerStats>()?.Initialize();

        OnHealthChanged?.Invoke();
    }

    // =========================================
    // 8 DIRECTIONS SYSTEM
    // =========================================

    public void UpdateDirection(Vector2 direction)
    {
        if (currentHero == null || bodyRenderer == null)
            return;

        if (currentHero.directionalSprites == null)
            return;

        if (direction == Vector2.zero)
            return;

        var sprites = currentHero.directionalSprites;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f)
            bodyRenderer.sprite = sprites.RightView;

        else if (angle >= 22.5f && angle < 67.5f)
            bodyRenderer.sprite = sprites.FrontRightView;

        else if (angle >= 67.5f && angle < 112.5f)
            bodyRenderer.sprite = sprites.FrontView;

        else if (angle >= 112.5f && angle < 157.5f)
            bodyRenderer.sprite = sprites.FrontLeftView;

        else if (angle >= 157.5f || angle < -157.5f)
            bodyRenderer.sprite = sprites.LeftView;

        else if (angle >= -157.5f && angle < -112.5f)
            bodyRenderer.sprite = sprites.BackLeftView;

        else if (angle >= -112.5f && angle < -67.5f)
            bodyRenderer.sprite = sprites.BackView;

        else if (angle >= -67.5f && angle < -22.5f)
            bodyRenderer.sprite = sprites.BackRightView;
    }

    public void UpdateLookDirection(Vector2 direction, bool isMoving)
    {
        if (currentHero == null || bodyRenderer == null)
            return;

        if (currentHero.directionalSprites == null)
            return;

        if (direction != Vector2.zero)
            lastLookDirection = direction.normalized;

        if (isMoving)
            idleTimer = 0f;
        else
            idleTimer += Time.deltaTime;

        // 🔹 Si lleva 3s quieto → mirar al frente
        if (idleTimer >= idleThreshold)
        {
            bodyRenderer.sprite =
                currentHero.directionalSprites.FrontView;
            return;
        }

        SetDirectionalSprite(lastLookDirection);
    }

    void SetDirectionalSprite(Vector2 direction)
    {
        var sprites = currentHero.directionalSprites;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f)
            bodyRenderer.sprite = sprites.RightView;

        else if (angle >= 22.5f && angle < 67.5f)
            bodyRenderer.sprite = sprites.BackRightView;

        else if (angle >= 67.5f && angle < 112.5f)
            bodyRenderer.sprite = sprites.BackView;

        else if (angle >= 112.5f && angle < 157.5f)
            bodyRenderer.sprite = sprites.BackLeftView;

        else if (angle >= 157.5f || angle < -157.5f)
            bodyRenderer.sprite = sprites.LeftView;

        else if (angle >= -157.5f && angle < -112.5f)
            bodyRenderer.sprite = sprites.FrontLeftView;

        else if (angle >= -112.5f && angle < -67.5f)
            bodyRenderer.sprite = sprites.FrontView;

        else if (angle >= -67.5f && angle < -22.5f)
            bodyRenderer.sprite = sprites.FrontRightView;
    }

    // =========================================
    // HEALTH MANAGEMENT
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
        if (currentHero.weapons.Count == 0) return;

        index = Mathf.Clamp(index, 0, currentHero.weapons.Count - 1);

        if (weapon == null) return;

        currentWeaponIndex = index;

        weapon.SetWeapon(currentHero.weapons[index]);
    }

    // =========================================
    // DEATH
    // =========================================

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Hero died");

        if (movement != null)
            movement.enabled = false;

        if (weapon != null)
            weapon.enabled = false;

        OnPlayerDeath?.Invoke();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.08f);

        Time.timeScale = 1f;

        yield return new WaitForSeconds(0.1f);

        GameManager.Instance.HandlePlayerDeath();
    }
}