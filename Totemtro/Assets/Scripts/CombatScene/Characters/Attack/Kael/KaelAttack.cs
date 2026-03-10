using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KaelAttack : MonoBehaviour
{
    PlayerStats stats;
    Rigidbody2D rb;
    PlayerMovement movement;
    Transform player;
    PlayerHealth playerHealth;

    [Header("Prefabs")]
    public GameObject fireXPrefab;
    public GameObject afterImagePrefab;

    [Header("Attack")]
    public float attackCooldown = 0.35f;
    public float spiralAngleStep = 18f;
    public float attackDuration = 1.25f;

    [Header("Attack Control")]
    public float enemyPushRadius = 1.4f;
    public float enemyPushForce = 6f;

    [Header("Dash")]
    public float dashForce = 500f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 2f;

    [Header("Spiral Settings")]
    public float spiralRadius = 2.5f;
    public float spawnDelay = 0.015f;

    [Header("Attack Animation")]
    public SpriteRenderer attackRenderer;
    public Sprite[] attackSprites;
    public float spriteAnimSpeed = 0.05f;

    [Header("Attack Rotation")]
    public float attackRotationSpeed = 900f;

    float lastAttackTime;
    float lastDashTime;

    bool isDashing = false;
    bool isAttacking = false;

    SpriteRenderer playerSprite;

    Coroutine spiralRoutine;
    Coroutine animRoutine;

    void Awake()
    {
        player = transform.parent;

        stats = GetComponentInParent<PlayerStats>();
        rb = GetComponentInParent<Rigidbody2D>();
        movement = GetComponentInParent<PlayerMovement>();
        playerHealth = GetComponentInParent<PlayerHealth>();

        playerSprite = player.GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (isAttacking && attackRenderer != null)
        {
            attackRenderer.transform.Rotate(
                0,
                0,
                -attackRotationSpeed * Time.deltaTime
            );
        }
    }

    // ================================
    // NORMAL ATTACK
    // ================================

    public void NormalAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        if (isDashing) return;
        if (isAttacking) return;

        lastAttackTime = Time.time;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        rb.linearVelocity = Vector2.zero;

        if (movement != null)
            movement.enabled = false;

        PushEnemies();

        isAttacking = true;

        spiralRoutine = StartCoroutine(SpiralAttack());
        animRoutine = StartCoroutine(AttackAnimation());

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        if (spiralRoutine != null)
            StopCoroutine(spiralRoutine);

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        if (movement != null)
            movement.enabled = true;

        rb.linearVelocity = Vector2.zero;

        if (attackRenderer != null)
            attackRenderer.transform.rotation = Quaternion.identity;
    }

    IEnumerator AttackAnimation()
    {
        int frame = 0;

        while (isAttacking)
        {
            if (attackSprites.Length > 0)
            {
                attackRenderer.sprite = attackSprites[frame];

                frame++;

                if (frame >= attackSprites.Length)
                    frame = 0;
            }

            yield return new WaitForSeconds(spriteAnimSpeed);
        }
    }

    IEnumerator SpiralAttack()
    {
        Vector2 mouseDir = GetMouseDirection();

        float baseAngle =
            Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

        int i = 0;

        while (isAttacking)
        {
            float angle = baseAngle + spiralAngleStep * i;

            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            float radius = spiralRadius * Mathf.Sqrt(i * 0.1f);

            SpawnSlash(dir, radius);

            if (i % 3 == 0)
                SpawnAfterImage();

            i++;

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // ================================
    // DASH ATTACK
    // ================================

    public bool DashAttack()
    {
        if (Time.time < lastDashTime + dashCooldown) return false;
        if (isDashing) return false;
        if (isAttacking) return false;

        lastDashTime = Time.time;

        StartCoroutine(DashRoutine());
        return true;
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;

        Vector2 dir = GetMouseDirection();

        if (movement != null)
            movement.enabled = false;

        if (playerHealth != null)
            playerHealth.SetInvulnerable(true);

        float timer = 0f;

        float dashSpiralDelay = 0.005f;
        float spiralTimer = 0f;
        int i = 0;

        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        while (timer < dashDuration)
        {
            rb.MovePosition(
                rb.position + dir * dashForce * Time.deltaTime
            );

            spiralTimer += Time.deltaTime;

            if (spiralTimer >= dashSpiralDelay)
            {
                float angle = spiralAngleStep * i;

                Vector2 spiralDir = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                float radius = spiralRadius * Mathf.Sqrt(i * 0.1f);

                SpawnSlash(spiralDir, radius);

                spiralTimer = 0f;
                i++;
            }

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(player.position, 1.2f);

            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null) continue;

                if (hitEnemies.Contains(enemy)) continue;

                enemy.TakeDamage(stats.Damage * 1.4f, dir, false);

                Rigidbody2D enemyRB = enemy.GetComponent<Rigidbody2D>();

                if (enemyRB != null)
                {
                    enemyRB.AddForce(
                        dir * enemyPushForce * 3f,
                        ForceMode2D.Impulse
                    );
                }

                hitEnemies.Add(enemy);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (movement != null)
            movement.enabled = true;

        if (playerHealth != null)
            playerHealth.SetInvulnerable(false);

        rb.linearVelocity = Vector2.zero;

        isDashing = false;
    }

    // ================================
    // HELPERS
    // ================================

    void SpawnSlash(Vector2 dir, float radius)
    {
        if (fireXPrefab == null) return;

        GameObject slash =
            Instantiate(fireXPrefab, player.position, Quaternion.identity);

        FireX fx = slash.GetComponent<FireX>();

        if (fx != null)
            fx.Initialize(stats.Damage, dir, player, radius);
    }

    void SpawnAfterImage()
    {
        if (afterImagePrefab == null || playerSprite == null) return;

        GameObject img =
            Instantiate(afterImagePrefab, player.position, player.rotation);

        AfterImage ai = img.GetComponent<AfterImage>();

        if (ai != null)
            ai.Initialize(playerSprite);
    }

    void PushEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(player.position, enemyPushRadius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Rigidbody2D enemyRB = hit.GetComponent<Rigidbody2D>();

            if (enemyRB == null) continue;

            Vector2 pushDir =
                (hit.transform.position - player.position).normalized;

            enemyRB.AddForce(
                pushDir * enemyPushForce,
                ForceMode2D.Impulse
            );
        }
    }

    Vector2 GetMouseDirection()
    {
        return (
            Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - player.position
        ).normalized;
    }
}