using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlimePuddle : MonoBehaviour
{
    [Header("Damage")]
    public float duration = 5f;
    public float tickInterval = 0.5f;
    public float damagePerTick = 4f;

    [Header("Slow")]
    public float slowPercent = 0.4f;
    public float slowDuration = 0.6f;

    [Header("Growth")]
    public float maxScaleMultiplier = 1.5f;
    public float growSpeed = 3f;
    public float shrinkDuration = 0.4f;

    [Header("Breathing")]
    public float breatheAmount = 0.05f;
    public float breatheSpeed = 2f;

    HashSet<PlayerHealth> playersInside = new();
    Vector3 baseScale;
    SpriteRenderer sr;
    bool isShrinking = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        baseScale = transform.localScale * maxScaleMultiplier;
        transform.localScale = Vector3.zero;

        StartCoroutine(Grow());
        StartCoroutine(TickRoutine());
        StartCoroutine(LifeCycle());
    }

    void Update()
    {
        if (isShrinking) return;

        // Respiración suave
        float breathe =
            1f + Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;

        transform.localScale = baseScale * breathe;
    }

    IEnumerator Grow()
    {
        float t = 0f;

        while (t < 1f)
        {
            transform.localScale =
                Vector3.Lerp(Vector3.zero, baseScale, t);

            t += Time.deltaTime * growSpeed;
            yield return null;
        }

        transform.localScale = baseScale;
    }

    IEnumerator LifeCycle()
    {
        yield return new WaitForSeconds(duration - shrinkDuration);

        yield return StartCoroutine(Shrink());

        Destroy(gameObject);
    }

    IEnumerator Shrink()
    {
        isShrinking = true;

        Vector3 startScale = transform.localScale;
        float startAlpha = sr != null ? sr.color.a : 1f;

        float t = 0f;

        while (t < 1f)
        {
            transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, t);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(startAlpha, 0f, t);
                sr.color = c;
            }

            t += Time.deltaTime / shrinkDuration;
            yield return null;
        }
    }

    IEnumerator TickRoutine()
    {
        while (true)
        {
            foreach (var player in playersInside)
            {
                if (player == null) continue;

                player.TakeDamage(
                    damagePerTick,
                    (player.transform.position - transform.position).normalized
                );

                PlayerMovement move =
                    player.GetComponent<PlayerMovement>();

                if (move != null)
                    move.ApplySlow(slowPercent, slowDuration);
            }

            yield return new WaitForSeconds(tickInterval);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null)
            playersInside.Add(player);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null)
            playersInside.Remove(player);
    }
}
