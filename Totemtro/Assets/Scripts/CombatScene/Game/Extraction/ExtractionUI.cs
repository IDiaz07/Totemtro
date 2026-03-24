using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExtractionUI : MonoBehaviour
{
    public static ExtractionUI Instance;

    [Header("Countdown Bar (pre-zone)")]
    public GameObject countdownRoot;
    public Image countdownFillBar;

    [Header("Channel Bar (in-zone)")]
    public GameObject channelRoot;
    public Image channelFillBar;
    public TMP_Text channelTimerText;

    [Header("Animation")]
    public float punchScale = 1.3f;
    public float punchDuration = 0.15f;

    float countdownTotal;
    float channelTotal;
    int lastSecond = -1;

    void Awake()
    {
        Instance = this;

        if (countdownRoot != null) countdownRoot.SetActive(false);
        if (channelRoot != null) channelRoot.SetActive(false);
    }

    // =========================================
    // COUNTDOWN (20s antes de la zona)
    // =========================================

    public void Show(float duration)
    {
        countdownTotal = duration;

        if (countdownRoot != null)
            countdownRoot.SetActive(true);

        if (countdownFillBar != null)
            countdownFillBar.fillAmount = 0f;
    }

    public void UpdateBar(float current)
    {
        if (countdownFillBar != null)
            countdownFillBar.fillAmount = current / countdownTotal;
    }

    public void Hide()
    {
        if (countdownRoot != null)
            countdownRoot.SetActive(false);
    }

    // =========================================
    // CHANNEL (10s dentro de la zona)
    // =========================================

    public void ShowChannel(float duration)
    {
        channelTotal = duration;
        lastSecond = -1;

        if (channelRoot != null)
            channelRoot.SetActive(true);

        if (channelFillBar != null)
            channelFillBar.fillAmount = 0f;

        if (channelTimerText != null)
        {
            int seconds = Mathf.CeilToInt(duration);
            channelTimerText.text = seconds.ToString();
        }
    }

    public void UpdateChannel(float current)
    {
        if (channelFillBar != null)
            channelFillBar.fillAmount = current / channelTotal;

        // Contador con animación cada segundo
        float remaining = channelTotal - current;
        int seconds = Mathf.CeilToInt(remaining);

        if (seconds != lastSecond && seconds >= 0)
        {
            lastSecond = seconds;

            if (channelTimerText != null)
            {
                channelTimerText.text = seconds.ToString();
                StopAllCoroutines();
                StartCoroutine(PunchText());
            }
        }
    }

    public void HideChannel()
    {
        if (channelRoot != null)
            channelRoot.SetActive(false);

        lastSecond = -1;
    }

    // =========================================
    // ANIMACIÓN DEL CONTADOR
    // =========================================

    IEnumerator PunchText()
    {
        if (channelTimerText == null)
            yield break;

        Transform t = channelTimerText.transform;
        Vector3 orig = Vector3.one;

        t.localScale = orig * punchScale;

        float elapsed = 0f;

        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / punchDuration;

            // Ease-out: rápido al principio, lento al final
            float eased = 1f - (1f - progress) * (1f - progress);

            t.localScale = Vector3.Lerp(orig * punchScale, orig, eased);
            yield return null;
        }

        t.localScale = orig;
    }
}