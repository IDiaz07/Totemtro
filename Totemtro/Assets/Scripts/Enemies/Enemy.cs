using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    public float contactDamage = 10f;

    [Header("Knockback")]
    public float hitKnockbackForce = 4f;
    public float hitKnockbackDuration = 0.12f;
    [Range(0f, 1f)]
    public float knockbackResistance = 0f;

    [Header("Hit Flash")]
    public float flashDuration = 0.08f;

    [Header("Drops")]
    public GameObject xpPrefab;
    public GameObject goldPrefab;
    public int xpAmount = 3;
    public int goldAmount = 2;
    public float dropForce = 3f;

    [Header("FX")]
    public GameObject hitParticlesPrefab;

    float currentHealth;

    DamageNumberSpawner damageSpawner;
    SpriteRenderer sprite;
    Color originalColor;

    Rigidbody2D rb;
    bool isKnocked = false;
    Coroutine knockbackRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageSpawner = GetComponent<DamageNumberSpawner>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
            originalColor = sprite.color;
    }

    // =====================================================
    // TAKE DAMAGE
    // =====================================================

    public void TakeDamage(float amount, Vector2 hitDirection, bool isCritical)
    {
        currentHealth -= amount;

        SpawnHitParticles(isCritical);

        StartCoroutine(HitFlash());
        StartCoroutine(DamageSquash());

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(HitKnockback(hitDirection));

        if (damageSpawner != null)
            damageSpawner.SpawnDamage(amount, isCritical);

        if (currentHealth <= 0f)
            Die();
    }

    void SpawnHitParticles(bool isCritical)
    {
        if (hitParticlesPrefab == null) return;

        GameObject p = Instantiate(
            hitParticlesPrefab,
            transform.position,
            Quaternion.identity
        );
        Destroy(p, 1f);

        if (isCritical)
        {
            ParticleSystem ps = p.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startSizeMultiplier *= 1.4f;
                main.startSpeedMultiplier *= 1.5f;
            }
        }
    }

    // =====================================================
    // DEATH
    // =====================================================

    void Die()
    {
        SpawnDrops();
        StartCoroutine(DeathAnimation());
    }

    void SpawnDrops()
    {
        // XP
        for (int i = 0; i < xpAmount; i++)
        {
            SpawnSingleDrop(xpPrefab);
        }

        // GOLD
        for (int i = 0; i < goldAmount; i++)
        {
            SpawnSingleDrop(goldPrefab);
        }
    }

    void SpawnSingleDrop(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject drop = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity
        );

        Rigidbody2D dropRb = drop.GetComponent<Rigidbody2D>();

        if (dropRb != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            dropRb.AddForce(randomDir * dropForce, ForceMode2D.Impulse);
        }
    }

    // =====================================================
    // CONTACT DAMAGE
    // =====================================================

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
        if (player == null) return;

        Vector2 hitDir =
            (collision.transform.position - transform.position).normalized;

        player.TakeDamage(contactDamage, hitDir);
    }

    // =====================================================
    // KNOCKBACK
    // =====================================================

    IEnumerator HitKnockback(Vector2 hitDirection)
    {
        if (rb == null) yield break;

        isKnocked = true;
        rb.linearVelocity = Vector2.zero;

        Vector2 forceDir = hitDirection.normalized;
        float finalForce = hitKnockbackForce * (1f - knockbackResistance);

        rb.AddForce(forceDir * finalForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(hitKnockbackDuration);

        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    // =====================================================
    // VISUAL FX
    // =====================================================

    IEnumerator HitFlash()
    {
        if (sprite == null) yield break;

        sprite.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        sprite.color = originalColor;
    }

    IEnumerator DamageSquash()
    {
        Transform body = transform.Find("Body");
        if (body == null) yield break;

        Vector3 originalScale = body.localScale;

        Vector3 squashScale = new Vector3(
            originalScale.x * 1.2f,
            originalScale.y * 0.8f,
            originalScale.z
        );

        body.localScale = squashScale;

        yield return new WaitForSeconds(0.08f);

        body.localScale = originalScale;
    }

    IEnumerator DeathAnimation()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        float duration = 0.2f;
        float timer = 0f;

        Vector3 startScale = transform.localScale;

        while (timer < duration)
        {
            float t = timer / duration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            transform.Rotate(0, 0, 720f * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public bool IsKnocked()
    {
        return isKnocked;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

}
