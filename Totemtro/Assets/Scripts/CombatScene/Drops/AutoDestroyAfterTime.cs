using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class AutoDestroyAfterTime : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 35f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Warning Phase")]
    [SerializeField] private float warningDuration = 5f;
    [SerializeField] private float blinkSpeed = 8f;
    [SerializeField] private bool shrinkOnWarning = true;
    [SerializeField] private float shrinkAmount = 0.15f;

    [Header("Fade")]
    [SerializeField] private float finalFadeDuration = 0.5f;

    [Header("Optional Audio")]
    [SerializeField] private AudioSource destroySound;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        StartCoroutine(LifeRoutine());
    }

    IEnumerator LifeRoutine()
    {
        float timer = 0f;

        while (timer < lifetime - warningDuration)
        {
            timer += useUnscaledTime ?
                Time.unscaledDeltaTime :
                Time.deltaTime;

            yield return null;
        }

        yield return StartCoroutine(WarningPhase());

        yield return StartCoroutine(FadeAndDestroy());
    }

    IEnumerator WarningPhase()
    {
        float timer = 0f;

        while (timer < warningDuration)
        {
            timer += useUnscaledTime ?
                Time.unscaledDeltaTime :
                Time.deltaTime;

            float t = timer / warningDuration;

            // Blink progresivo
            if (spriteRenderer != null)
            {
                float blink = Mathf.Abs(Mathf.Sin(timer * blinkSpeed));
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0.2f, blink);
                spriteRenderer.color = c;
            }

            // Shrink suave
            if (shrinkOnWarning)
            {
                float scaleFactor = 1f - (shrinkAmount * t);
                transform.localScale = originalScale * scaleFactor;
            }

            yield return null;
        }
    }

    IEnumerator FadeAndDestroy()
    {
        if (destroySound != null)
            destroySound.Play();

        float timer = 0f;

        while (timer < finalFadeDuration)
        {
            timer += useUnscaledTime ?
                Time.unscaledDeltaTime :
                Time.deltaTime;

            float t = timer / finalFadeDuration;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(spriteRenderer.color.a, 0f, t);
                spriteRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}