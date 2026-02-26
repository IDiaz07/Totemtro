using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KaelAttack : MonoBehaviour
{
    PlayerStats playerStats;

    [Header("Damage")]
    public float normalDamage = 11f;
    public float dashDamage = 10f;

    [Header("Cooldowns")]
    public float normalCooldown = 0.4f;
    public float dashCooldown = 3f;

    [Header("Dash Settings")]
    public float dashForce = 18f;
    public float dashDuration = 0.15f;

    [Header("Invulnerability")]
    public float dashInvulnerabilityDuration = 7f;

    [Header("Slash Animation")]
    public float slashAngle = 100f;
    public float slashSpeed = 900f;

    float lastNormalTime;
    float lastDashTime;

    Rigidbody2D rb;
    PlayerHealth playerHealth;
    PlayerMovement movement;
    TrailRenderer dashTrail;

    bool isDashing = false;
    int originalLayer;

    HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        playerHealth = GetComponentInParent<PlayerHealth>();
        movement = GetComponentInParent<PlayerMovement>();
        dashTrail = GetComponentInParent<TrailRenderer>();
        playerStats = GetComponentInParent<PlayerStats>(); // 👈 AÑADIR
    }

    // =========================================
    // 🗡 ATAQUE NORMAL (2 golpes)
    // =========================================

    public void NormalAttack()
    {
        if (Time.time < lastNormalTime + normalCooldown) return;
        if (isDashing) return;

        lastNormalTime = Time.time;
        StartCoroutine(DoubleSlash());
    }

    IEnumerator DoubleSlash()
    {
        yield return StartCoroutine(SlashAnimation(false));
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(SlashAnimation(true));
    }

    IEnumerator SlashAnimation(bool applyHitStop)
    {
        Vector2 dir = GetMouseDirection();

        float baseAngle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle + slashAngle * 0.5f;
        float endAngle = baseAngle - slashAngle * 0.5f;

        float currentAngle = startAngle;
        float timer = 0f;

        while (Mathf.Abs(currentAngle - endAngle) > 1f)
        {
            currentAngle = Mathf.MoveTowards(
                currentAngle,
                endAngle,
                slashSpeed * Time.deltaTime
            );

            transform.rotation =
                Quaternion.Euler(0f, 0f, currentAngle);

            timer += Time.deltaTime;
            yield return null;
        }

        // Aplicamos daño justo al final del corte
        PerformSlash(applyHitStop);

        // Reset rotación
        transform.rotation =
            Quaternion.Euler(0f, 0f, baseAngle);
    }


    void PerformSlash(bool applyHitStop)
    {
        Vector2 dir = GetMouseDirection();

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.parent.position + (Vector3)(dir * 0.8f),
                0.8f
            );

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            float finalDamage = playerStats != null
                ? playerStats.Damage
                : normalDamage;

            enemy.TakeDamage(finalDamage, dir, false);


            if (applyHitStop)
                StartCoroutine(HitStop(0.06f));
        }
    }

    IEnumerator HitStop(float duration)
    {
        float original = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = original;
    }

    // =========================================
    // ⚡ DASH
    // =========================================

    public void DashAttack()
    {
        if (Time.time < lastDashTime + dashCooldown) return;
        if (isDashing) return;

        lastDashTime = Time.time;
        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        hitEnemies.Clear();

        Vector2 dir = GetMouseDirection();
        Transform player = transform.parent;

        // Cambiar layer para atravesar enemigos
        originalLayer = player.gameObject.layer;
        player.gameObject.layer = LayerMask.NameToLayer("PlayerDash");

        StartCoroutine(DashInvulnerability());

        if (movement != null)
            movement.enabled = false;

        if (dashTrail != null)
            dashTrail.enabled = true;

        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.MovePosition(
                rb.position + dir * dashForce * Time.deltaTime
            );

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    player.position,
                    0.8f
                );

            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null) continue;

                if (hitEnemies.Contains(enemy)) continue;

                // Empuje perpendicular
                Vector2 perpendicular = new Vector2(-dir.y, dir.x);
                float side = Random.value > 0.5f ? 1f : -1f;
                Vector2 launchDir = perpendicular * side;

                float finalDashDamage = playerStats != null
                    ? playerStats.Damage * 1.2f
                    : dashDamage;

                enemy.TakeDamage(finalDashDamage, launchDir, false);

                hitEnemies.Add(enemy);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Restaurar estado
        player.gameObject.layer = originalLayer;

        if (movement != null)
            movement.enabled = true;

        if (dashTrail != null)
            dashTrail.enabled = false;

        isDashing = false;
    }

    IEnumerator DashInvulnerability()
    {
        if (playerHealth == null) yield break;

        playerHealth.SetInvulnerable(true);

        yield return new WaitForSeconds(dashInvulnerabilityDuration);

        playerHealth.SetInvulnerable(false);
    }


    // =========================================

    Vector2 GetMouseDirection()
    {
        return (
            Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.parent.position
        ).normalized;
    }
}
