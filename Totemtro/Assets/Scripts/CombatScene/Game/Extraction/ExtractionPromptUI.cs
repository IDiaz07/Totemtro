using UnityEngine;
using TMPro;
using System.Collections;

public class ExtractionPromptUI : MonoBehaviour
{
    public static ExtractionPromptUI Instance;

    [Header("UI")]
    public GameObject root;
    public TMP_Text keyText;
    public TMP_Text labelText;

    [Header("Denied Message")]
    public TMP_Text deniedText;
    public float deniedDisplayTime = 2f;

    [Header("Shake")]
    public float shakeDuration = 0.4f;
    public float shakeMagnitude = 12f;

    [Header("Colors")]
    public Color deniedColor = Color.red;
    public Color normalColor = Color.white;

    Vector3 originalPos;
    Coroutine deniedRoutine;

    void Awake()
    {
        Instance = this;
        originalPos = root.transform.localPosition;

        if (labelText != null)
            labelText.text = "Extracción";

        if (deniedText != null)
        {
            deniedText.text = "";
            deniedText.alpha = 0f;
        }
    }

    public void SetKeyText(string key)
    {
        if (keyText != null)
            keyText.text = "[" + key + "]";
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    // =========================================
    // SHAKE + MENSAJE DE DENEGACIÓN
    // =========================================

    public void ShakeDenied(string reason = null)
    {
        StopAllCoroutines();

        if (deniedRoutine != null)
            StopCoroutine(deniedRoutine);

        StartCoroutine(ShakeRoutine());

        if (reason != null && deniedText != null)
            deniedRoutine = StartCoroutine(ShowDeniedMessage(reason));
    }

    IEnumerator ShakeRoutine()
    {
        if (keyText != null)
            keyText.color = deniedColor;

        if (labelText != null)
            labelText.color = deniedColor;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            root.transform.localPosition =
                originalPos + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        root.transform.localPosition = originalPos;

        if (keyText != null)
            keyText.color = normalColor;

        if (labelText != null)
            labelText.color = normalColor;
    }

    IEnumerator ShowDeniedMessage(string message)
    {
        deniedText.text = message;
        deniedText.color = deniedColor;

        // Fade in
        float fadeIn = 0.2f;
        float t = 0f;

        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            deniedText.alpha = t / fadeIn;
            yield return null;
        }

        deniedText.alpha = 1f;

        // Mantener visible
        yield return new WaitForSecondsRealtime(deniedDisplayTime);

        // Fade out
        float fadeOut = 0.5f;
        t = 0f;

        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            deniedText.alpha = 1f - (t / fadeOut);
            yield return null;
        }

        deniedText.alpha = 0f;
        deniedText.text = "";
        deniedRoutine = null;
    }
}