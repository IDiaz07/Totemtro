using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class RunSummaryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private Image separator;
    [SerializeField] private GameObject continueButton;

    [Header("Audio")]
    [SerializeField] private AudioSource tickAudio;
    [SerializeField] private AudioSource finalHitAudio;

    [Header("Animation")]
    [SerializeField] private float panelIntroDuration = 0.35f;
    [SerializeField] private float numberRevealDelay = 0.4f;

    [Header("Gold Particles")]
    [SerializeField] private ParticleSystem goldConeParticles;
    [SerializeField] private ParticleSystem goldFinalBurstParticles;
    [SerializeField] private Transform backpackTarget;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Sprite goldIconSprite;

    private RectTransform panelRect;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show(bool extracted)
    {
        gameObject.SetActive(true);

        if (RunSummaryManager.Instance == null ||
            RunSummaryManager.Instance.LastRun == null)
        {
            Debug.LogError("RunSummary data missing");
            return;
        }

        var data = RunSummaryManager.Instance.LastRun;

        headerText.text = "EXTRACTION SUCCESSFUL";
        headerText.color = new Color(0.6f, 1f, 0.6f);

        // Reset estado visual
        timeText.alpha = 0;
        killsText.alpha = 0;
        goldText.alpha = 0;
        separator.color = new Color(1, 1, 1, 0);
        continueButton.SetActive(false);

        StartCoroutine(CinematicSequence(data));
    }

    IEnumerator CinematicSequence(RunSummaryData data)
    {
        yield return StartCoroutine(AnimatePanelIntro());

        yield return new WaitForSecondsRealtime(0.2f);

        timeText.text = "Time Survived\n" +
            Mathf.FloorToInt(data.timeSurvived) + "s";

        yield return StartCoroutine(FadeText(timeText));
        yield return new WaitForSecondsRealtime(numberRevealDelay);

        killsText.text = "Enemies Slain\n" + data.enemiesKilled;

        yield return StartCoroutine(FadeText(killsText));
        yield return new WaitForSecondsRealtime(numberRevealDelay);

        yield return StartCoroutine(FadeImage(separator));
        yield return new WaitForSecondsRealtime(0.2f);

        yield return StartCoroutine(AnimateGold(data.totalReward));
        yield return StartCoroutine(PunchGold());

        continueButton.SetActive(true);
    }

    // =========================================
    // BUTTON CALLBACKS
    // =========================================

    public void OnContinueButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ReturnToHub();
    }

    // =========================================
    // ANIMATIONS
    // =========================================

    IEnumerator AnimatePanelIntro()
    {
        canvasGroup.alpha = 0;
        panelRect.localScale = Vector3.zero;

        float t = 0f;

        while (t < panelIntroDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / panelIntroDuration;
            float eased = Mathf.SmoothStep(0, 1, progress);

            canvasGroup.alpha = eased;
            panelRect.localScale = Vector3.one * eased;

            yield return null;
        }

        canvasGroup.alpha = 1;
        panelRect.localScale = Vector3.one;
    }

    IEnumerator FadeText(TMP_Text text)
    {
        float t = 0f;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            text.alpha = t / duration;
            yield return null;
        }

        text.alpha = 1;
    }

    IEnumerator FadeImage(Image img)
    {
        float t = 0f;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Color c = img.color;
            c.a = t / duration;
            img.color = c;
            yield return null;
        }
    }

    IEnumerator PunchGold()
    {
        Vector3 original = goldText.transform.localScale;

        goldText.transform.localScale = original * 1.2f;
        yield return new WaitForSecondsRealtime(0.08f);
        goldText.transform.localScale = original;
    }

    IEnumerator AnimateGold(int finalValue)
    {
        int current = 0;

        float duration = Mathf.Clamp(finalValue / 500f, 1.5f, 4.5f);
        float timer = 0f;

        goldText.text = "Reward\n0 Gold";
        goldText.alpha = 1;

        float glowStrength = Mathf.Clamp01(finalValue / 2000f);
        goldText.fontMaterial.SetFloat("_GlowPower", glowStrength * 0.5f);

        if (goldConeParticles != null)
        {
            var main = goldConeParticles.main;
            main.duration = duration;
            main.startLifetime = duration;

            var emission = goldConeParticles.emission;
            emission.rateOverTime = Mathf.Clamp(finalValue / 10f, 30f, 400f);

            goldConeParticles.Play();
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = timer / duration;
            float eased = Mathf.SmoothStep(0, 1, progress);

            int value = Mathf.FloorToInt(Mathf.Lerp(0, finalValue, eased));

            if (value != current)
            {
                current = value;
                goldText.text = "Reward\n" + current + " Gold";

                if (tickAudio != null)
                    tickAudio.Play();
            }

            yield return null;
        }

        goldText.text = "Reward\n" + finalValue + " Gold";

        if (goldConeParticles != null)
            goldConeParticles.Stop();

        yield return StartCoroutine(FinalBurst(finalValue));
    }

    IEnumerator FinalBurst(int finalValue)
    {
        int burstCount = Mathf.Clamp(finalValue / 20, 10, 150);

        if (goldFinalBurstParticles != null)
        {
            var emission = goldFinalBurstParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)burstCount)
            });

            goldFinalBurstParticles.Play();
        }

        if (finalHitAudio != null)
            finalHitAudio.Play();

        yield return new WaitForSecondsRealtime(1.5f);
    }
}