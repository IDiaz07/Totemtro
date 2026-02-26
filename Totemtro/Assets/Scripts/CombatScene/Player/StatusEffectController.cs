using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusEffectController : MonoBehaviour
{
    Dictionary<string, Coroutine> activeEffects =
        new Dictionary<string, Coroutine>();

    PlayerStats stats;
    PlayerHealth health;

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
        if (activeEffects.ContainsKey(key))
        {
            StopCoroutine(activeEffects[key]);
            activeEffects.Remove(key);
        }

        Coroutine c = StartCoroutine(routine);
        activeEffects.Add(key, c);
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

            // Cancelar si se mueve
            if (movement != null && movement.IsMoving())
            {
                cancelled = true;
                break;
            }

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
            // ✅ Curación exitosa
            health.Heal(amount);

            ActionBarController actionBar =
                FindFirstObjectByType<ActionBarController>();

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