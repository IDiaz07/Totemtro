using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BuffIconUI : MonoBehaviour
{
    [Header("UI")]
    public Image radialBorder;
    public Image glowImage;
    public TMP_Text timeText;
    public CanvasGroup canvasGroup;

    float duration;
    float timer;

    string buffKey;
    Action<string> onFinished;

    bool isEnding = false;
    bool popFinished = false;

    float popTimer = 0f;
    float popDuration = 0.18f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (radialBorder == null)
            radialBorder = GetComponentInChildren<Image>();

        if (timeText == null)
            timeText = GetComponentInChildren<TMP_Text>();
    }

    public void Initialize(string key, float buffDuration, Action<string> callback)
    {
        buffKey = key;
        duration = buffDuration;
        timer = buffDuration;
        onFinished = callback;

        popFinished = false;
        popTimer = 0f;

        transform.localScale = Vector3.zero;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void Refresh(float newDuration)
    {
        duration = newDuration;
        timer = newDuration;
    }

    void Update()
    {
        if (duration <= 0f)
            return;

        // 🔥 POP ANIMATION
        if (!popFinished)
        {
            popTimer += Time.deltaTime;
            float t = popTimer / popDuration;

            float scale = Mathf.Lerp(0f, 1.1f, t);
            transform.localScale = Vector3.one * scale;

            if (t >= 1f)
            {
                popFinished = true;
                transform.localScale = Vector3.one;
            }

            return;
        }

        if (isEnding)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartFadeOut();
            return;
        }

        // 🔥 SAFE UI UPDATE
        if (radialBorder != null)
            radialBorder.fillAmount = timer / duration;

        if (timeText != null)
            timeText.text = timer.ToString("0.0");

        // 🔥 Parpadeo < 3s
        if (timer <= 3f && glowImage != null)
        {
            float pulse = Mathf.PingPong(Time.time * 6f, 1f);
            glowImage.color = new Color(1f, 1f, 1f, pulse);
        }
    }

    void StartFadeOut()
    {
        if (isEnding)
            return;

        isEnding = true;
        StartCoroutine(FadeRoutine());
    }

    System.Collections.IEnumerator FadeRoutine()
    {
        float t = 0f;
        float fadeDuration = 0.25f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f - (t / fadeDuration);

            yield return null;
        }

        onFinished?.Invoke(buffKey);
        Destroy(gameObject);   // ✅ ESTO ES LO CORRECTO
    }

    // ========================================
    // ULTRA PRO CANCEL
    // ========================================

    [Header("Cancel FX")]
    public ParticleSystem breakParticles;
    public AudioClip cancelSound;
    public Image crackOverlay;

    public void Cancel()
    {
        if (isEnding)
            return;

        isEnding = true;
        StartCoroutine(CancelRoutine());
    }

    System.Collections.IEnumerator CancelRoutine()
    {
        float t = 0f;
        float duration = 0.25f;

        Vector3 originalScale = transform.localScale;

        // 🔥 Micro hit stop
        float originalTime = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.04f);
        Time.timeScale = originalTime;

        // 🔥 Camera shake (tu versión correcta)
        CameraShake.ShakeCamera(0.08f, 0.15f);

        // 🔥 Sonido seco
        if (cancelSound != null)
            AudioSource.PlayClipAtPoint(
                cancelSound,
                Camera.main.transform.position,
                0.7f
            );

        // 🔥 Partículas
        if (breakParticles != null)
            Instantiate(
                breakParticles,
                transform.position,
                Quaternion.identity,
                transform.parent
            );

        // 🔥 Crack overlay
        if (crackOverlay != null)
            crackOverlay.enabled = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float scale = Mathf.Lerp(1f, 0f, t / duration);
            transform.localScale = originalScale * scale;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f - (t / duration);

            yield return null;
        }

        onFinished?.Invoke(buffKey);
        Destroy(gameObject);
    }
}