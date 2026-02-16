using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Damage Feedback")]
    public float flashDuration = 0.1f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.1f;

    [Header("Defensive Stats")]
    public float damageReductionPercent = 0f; // 0.2 = 20%
    public float dodgeChance = 0f;            // 0.15 = 15%

    SpriteRenderer sprite;
    Color originalColor;
    Rigidbody2D rb;

    DamageNumberSpawner damageSpawner;
    HeroController heroController;

    bool isInvulnerable = false;

    public float healthRegenPerSecond = 0f;
    public float shieldAmount = 0f;
    public bool hasSecondWind = false;
    bool secondWindUsed = false;

    public float retaliationMultiplier = 0f;


    void Update()
    {
        if (heroController == null) return;

        if (healthRegenPerSecond > 0f &&
            heroController.CurrentHealth < heroController.MaxHealth)
        {
            float newHealth =
                heroController.CurrentHealth +
                healthRegenPerSecond * Time.deltaTime;

            newHealth = Mathf.Min(newHealth, heroController.MaxHealth);

            heroController.SetCurrentHealth(newHealth);
        }
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        damageSpawner = GetComponent<DamageNumberSpawner>();
        heroController = GetComponent<HeroController>();

        if (sprite != null)
            originalColor = sprite.color;
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (isInvulnerable) return;
        if (heroController == null) return;

        float damage = amount;

        // =========================
        // 🛡 SHIELD
        // =========================
        if (shieldAmount > 0f)
        {
            float absorbed = Mathf.Min(shieldAmount, damage);
            shieldAmount -= absorbed;
            damage -= absorbed;

            if (damage <= 0f)
                return;
        }

        // =========================
        // 🎲 EVASION
        // =========================
        if (Random.value < dodgeChance)
        {
            Debug.Log("Dodged!");
            return;
        }

        // =========================
        // 🛡 DAMAGE REDUCTION
        // =========================
        damage *= (1f - damageReductionPercent);

        heroController.ApplyDamage(damage);

        if (damageSpawner != null)
            damageSpawner.SpawnDamage(damage, false);

        StartCoroutine(HitFlash());
        StartCoroutine(Knockback(hitDirection));
        StartCoroutine(Invulnerability(0.3f));

        // =========================
        // 🔥 RETALIATION
        // =========================
        TotemInventory inventory = FindFirstObjectByType<TotemInventory>();

        if (inventory != null && inventory.HasTotem(TotemType.Retaliation))
        {
            Collider2D[] enemies =
                Physics2D.OverlapCircleAll(transform.position, 1.2f);

            foreach (var col in enemies)
            {
                if (!col.CompareTag("Enemy")) continue;

                Enemy e = col.GetComponent<Enemy>();
                if (e == null) continue;

                e.TakeDamage(damage * retaliationMultiplier, Vector2.zero, false);
            }
        }

        // =========================
        // 💀 CHECK DEATH + SECOND WIND
        // =========================
        if (heroController.CurrentHealth <= 0f)
        {
            if (hasSecondWind && !secondWindUsed)
            {
                secondWindUsed = true;

                heroController.SetCurrentHealth(
                    heroController.MaxHealth * 0.5f
                );

                return;
            }

            heroController.Die();
        }
    }

    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
    }

    public float GetCurrentHealthPercent()
    {
        if (heroController == null) return 1f;

        return heroController.CurrentHealth / heroController.MaxHealth;
    }


    IEnumerator Knockback(Vector2 dir)
    {
        if (rb == null) yield break;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;
    }

    IEnumerator HitFlash()
    {
        if (sprite == null) yield break;

        sprite.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        sprite.color = originalColor;
    }

    IEnumerator Invulnerability(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }
}
