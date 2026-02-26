using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class SpitterAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float attackRange = 6f;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public float windUpDuration = 0.4f;
    public GameObject projectilePrefab;

    [Header("Charge FX")]
    public float chargeShakeIntensity = 0.03f;
    public float chargeShakeSpeed = 40f;
    public AudioClip chargeSound;

    Transform player;
    Rigidbody2D rb;
    Enemy enemy;

    Transform body;
    SpriteRenderer bodyRenderer;
    AudioSource audioSource;

    bool canAttack = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        audioSource = GetComponent<AudioSource>();

        body = transform.Find("Body");
        if (body != null)
            bodyRenderer = body.GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (enemy.IsKnocked()) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > attackRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (canAttack)
                StartCoroutine(Shoot());
        }
    }

    IEnumerator Shoot()
    {
        canAttack = false;

        yield return StartCoroutine(WindUp());

        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

            proj.GetComponent<ArcProjectilePhysics>()
                .Init(player.position);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator WindUp()
    {
        if (body == null) yield break;

        Vector3 originalScale = body.localScale;
        Vector3 originalPos = body.localPosition;

        Vector3 windUpScale = new Vector3(
            originalScale.x * 0.8f,
            originalScale.y * 1.2f,
            originalScale.z
        );

        if (bodyRenderer != null)
            bodyRenderer.color = new Color(0.6f, 1f, 0.6f);

        if (audioSource != null && chargeSound != null)
            audioSource.PlayOneShot(chargeSound);

        float timer = 0f;

        while (timer < windUpDuration)
        {
            float t = timer / windUpDuration;

            body.localScale = Vector3.Lerp(originalScale, windUpScale, t);

            float shake =
                Mathf.Sin(Time.time * chargeShakeSpeed) * chargeShakeIntensity;

            body.localPosition = originalPos + Vector3.right * shake;

            timer += Time.deltaTime;
            yield return null;
        }

        body.localScale = originalScale;
        body.localPosition = originalPos;

        if (bodyRenderer != null)
            bodyRenderer.color = Color.white;
    }
}
