using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusEffectController : MonoBehaviour
{
    Dictionary<string, Coroutine> activeEffects =
        new Dictionary<string, Coroutine>();

    PlayerStats stats;
    PlayerHealth health;

    public GameObject healNumberPrefab;

    public AudioClip healFinishSound;
    AudioSource audioSource;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        health = GetComponent<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
    }

    // =========================
    // SPEED
    // =========================

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartEffect("Speed", SpeedRoutine(multiplier, duration));
    }

    IEnumerator SpeedRoutine(float multiplier, float duration)
    {
        if (stats == null)
            yield break;

        // Aplicar multiplicador
        stats.SetSpeedMultiplier(multiplier);

        BuffUI.Instance?.AddBuff("Speed", duration);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Restaurar velocidad
        stats.SetSpeedMultiplier(1f);
    }

    // =========================
    // ARMOR
    // =========================

    public void ApplyDamageReduction(float percent, float duration)
    {
        StartEffect("Armor", ArmorRoutine(percent, duration));
    }

    IEnumerator ArmorRoutine(float percent, float duration)
    {
        health.damageReductionPercent += percent;

        BuffUI.Instance?.AddBuff("Armor", duration);

        yield return new WaitForSeconds(duration);

        health.damageReductionPercent -= percent;
    }

    // =========================
    // REGEN OVER TIME
    // =========================

    public void ApplyRegen(int totalAmount, float duration)
    {
        StartEffect("Regen", RegenRoutine(totalAmount, duration));
    }

    IEnumerator RegenRoutine(int total, float duration)
    {
        if (health == null)
            yield break;

        BuffUI.Instance?.AddBuff("Regen", duration);

        int amountPerTick = 1;
        float interval = duration / total;

        float timer = 0f;

        while (timer < duration)
        {
            health.Heal(amountPerTick);
            yield return new WaitForSeconds(interval);
            timer += interval;
        }
    }

    // =========================
    // START EFFECT (NO STACK)
    // =========================

    void StartEffect(string key, IEnumerator routine)
    {
        if (routine == null)
        {
            Debug.LogWarning("StatusEffect routine is NULL: " + key);
            return;
        }

        if (activeEffects.TryGetValue(key, out Coroutine existing))
        {
            if (existing != null)
                StopCoroutine(existing);

            activeEffects.Remove(key);
        }

        Coroutine c = StartCoroutine(routine);

        if (c != null)
            activeEffects[key] = c;
    }

    // =========================
    // CAST HEAL (Bandage)
    // =========================

    public void ApplyCastHeal(int amount, float castTime)
    {
        StartEffect("Bandage", CastHealRoutine(amount, castTime));
    }

    IEnumerator CastHealRoutine(int amount, float castTime)
    {
        bool cancelled = false;

        void Cancel()
        {
            cancelled = true;
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        BandageAnimation bandageAnim = GetComponent<BandageAnimation>();

        // Suscribirse a eventos
        PlayerHealth.OnPlayerDamaged += Cancel;
        Weapon.OnPlayerShot += Cancel;

        // 🔥 Aplicar slow manual
        if (movement != null)
            movement.SetSlowMultiplier(0.4f); // 60% slow

        // 🔥 Activar animación procedural
        if (bandageAnim != null)
            bandageAnim.StartBandaging();

        // 🔥 Mostrar buff UI
        BuffUI.Instance?.AddBuff("Bandage", castTime);

        float timer = 0f;

        while (timer < castTime)
        {
            if (cancelled)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        // Desuscribirse
        PlayerHealth.OnPlayerDamaged -= Cancel;
        Weapon.OnPlayerShot -= Cancel;

        // 🔥 Restaurar velocidad SIEMPRE
        if (movement != null)
            movement.SetSlowMultiplier(1f);

        // 🔥 Restaurar animación SIEMPRE
        if (bandageAnim != null)
            bandageAnim.StopBandaging();

        if (!cancelled && timer >= castTime)
        {
            health.Heal(amount);

            ActionBarController actionBar =
                FindFirstObjectByType<ActionBarController>();

            if (actionBar != null)
                actionBar.ConsumeBandage();   // 👈 consumir aquí

            StartCoroutine(HitStop(0.05f));

            if (audioSource != null && healFinishSound != null)
                audioSource.PlayOneShot(healFinishSound);
        }
        else
        {
            // ❌ Cancelado → no consumir venda
            BuffUI.Instance?.CancelBuff("Bandage");
        }
    }

    public void ApplyPotionHeal(float castTime)
    {
        StartEffect("Potion", PotionRoutine(castTime));
    }

    IEnumerator PotionRoutine(float castTime)
    {
        bool cancelled = false;

        void Cancel()
        {
            cancelled = true;
        }

        PlayerHealth.OnPlayerDamaged += Cancel;
        Weapon.OnPlayerShot += Cancel;

        BuffUI.Instance?.AddBuff("Potion", castTime);

        float tickInterval = castTime / 5f;
        float timer = 0f;
        float tickTimer = 0f;

        int ticksDone = 0;

        while (timer < castTime)
        {
            if (cancelled)
                break;

            timer += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval && ticksDone < 5)
            {
                tickTimer = 0f;
                ticksDone++;

                HealTick(2);
            }

            yield return null;
        }

        PlayerHealth.OnPlayerDamaged -= Cancel;
        Weapon.OnPlayerShot -= Cancel;

        // terminó correctamente
        if (!cancelled && timer >= castTime)
        {
            HealTick(15);
        }
        else
        {
            // cancelar buff visual
            BuffUI.Instance?.CancelBuff("Potion");
        }

        // consumir poción siempre
        ActionBarController actionBar =
            FindFirstObjectByType<ActionBarController>();

        if (actionBar != null)
            actionBar.ConsumePotion();
    }

    void HealTick(int amount)
    {
        if (health == null)
            return;

        health.Heal(amount);

        if (healNumberPrefab != null)
        {
            Vector3 pos =
                transform.position +
                Vector3.up * 0.7f +
                new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f);

            GameObject obj =
                Instantiate(healNumberPrefab, pos, Quaternion.identity);

            HealNumber heal = obj.GetComponent<HealNumber>();

            if (heal != null)
                heal.SetHeal(amount);
        }
    }

    IEnumerator HitStop(float duration)
    {
        float originalTime = Time.timeScale;

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTime;
    }

    void MiniHitStop()
    {
        StartCoroutine(HitStopRoutine());
    }

    IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0.15f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;
    }
}