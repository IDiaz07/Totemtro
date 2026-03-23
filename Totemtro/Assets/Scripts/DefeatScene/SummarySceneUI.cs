using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SummarySceneUI : MonoBehaviour
{
    // =========================================
    // HEADER
    // =========================================

    [Header("Header")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text resultText2;
    [SerializeField] private Image triangleOverlay;

    // =========================================
    // BACKGROUND & FRAME
    // =========================================

    [Header("Background & Frame")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image frameImage;

    // =========================================
    // HERO
    // =========================================

    [Header("Hero Display")]
    [SerializeField] private Image heroIcon;

    // =========================================
    // TROPHIES
    // =========================================

    [Header("Trophies")]
    [SerializeField] private TMP_Text trophyDeltaText;

    // =========================================
    // RIGHT PANEL — TIMER / TOTAL GOLD
    // =========================================

    [Header("Timer Stat")]
    [SerializeField] private TMP_Text timerValueText;

    [Header("Total Gold Stat")]
    [SerializeField] private TMP_Text totalGoldValueText;

    // =========================================
    // BOTTOM PANEL — KILLS / HEAL / DAMAGE
    // =========================================

    [Header("Bottom Stats")]
    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private TMP_Text killsValueText;
    [SerializeField] private TMP_Text healValueText;
    [SerializeField] private TMP_Text damageValueText;

    // =========================================
    // MASTERY
    // =========================================

    [Header("Mastery")]
    [SerializeField] private Image masteryBarFill;
    [SerializeField] private Image masteryTierIcon;
    [SerializeField] private TMP_Text masteryTierText;
    [SerializeField] private TMP_Text masteryXPText;
    [SerializeField] private MasteryTierIcons masteryTierIcons;

    // =========================================
    // VICTORY BURST
    // =========================================

    [Header("Victory Burst")]
    [SerializeField] private ParticleSystem victoryBurstParticles;
    [SerializeField] private AudioSource victoryBurstAudio;

    // =========================================
    // BUTTON
    // =========================================

    [Header("Exit Button")]
    [SerializeField] private GameObject exitButton;
    [SerializeField] private CanvasGroup exitButtonCanvasGroup;

    // =========================================
    // AUDIO
    // =========================================

    [Header("Audio")]
    [SerializeField] private AudioSource countTickAudio;
    [SerializeField] private AudioSource impactAudio;
    [SerializeField] private AudioSource goldTotalAudio;
    [SerializeField] private AudioSource tierUpAudio;

    // =========================================
    // TIMING
    // =========================================

    [Header("Timing")]
    [SerializeField] private float countDuration = 1.2f;
    [SerializeField] private float statDelay = 0.3f;

    // =========================================
    // COLORS
    // =========================================

    [Header("Victory Colors")]
    [SerializeField] private Color victoryTextColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color victoryOverlayColor = new Color(0.15f, 0.35f, 0.8f, 0.8f);
    [SerializeField] private Color victoryBgColor = new Color(0.05f, 0.1f, 0.2f);
    [SerializeField] private Color victoryFrameColor = new Color(0.2f, 0.45f, 0.9f);

    [Header("Defeat Colors")]
    [SerializeField] private Color defeatTextColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color defeatOverlayColor = new Color(0.8f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color defeatBgColor = new Color(0.15f, 0.03f, 0.03f);
    [SerializeField] private Color defeatFrameColor = new Color(0.7f, 0.1f, 0.1f);

    // =========================================
    // INTERNAL
    // =========================================

    private RunSummaryData data;
    private bool isVictory;

    // Reward pendiente para animar en el Hub
    public static int PendingGoldReward;

    // =========================================
    // START
    // =========================================

    void Start()
    {
        Time.timeScale = 1f;

        data = RunSummaryManager.LastRunData;

        isVictory = data != null && data.extracted;

        // Guardar reward para animar en el Hub
        PendingGoldReward = data != null ? data.totalReward : 0;

        SetupHeader();
        LoadHeroVisuals();
        ResetAll();

        StartCoroutine(SummarySequence());
    }

    // =========================================
    // SETUP HEADER (DEFEAT vs VICTORY)
    // =========================================

    void SetupHeader()
    {
        string headerLabel = isVictory ? "VICTORY" : "DEFEAT";
        Color headerColor = isVictory ? victoryTextColor : defeatTextColor;

        if (resultText != null)
        {
            resultText.text = headerLabel;
            resultText.color = headerColor;
        }

        if (resultText2 != null)
        {
            resultText2.text = headerLabel;
            resultText2.color = new Color(0f, 0f, 0f, 0.8f);
        }

        if (triangleOverlay != null)
        {
            triangleOverlay.color = isVictory
                ? victoryOverlayColor
                : defeatOverlayColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = isVictory
                ? victoryBgColor
                : defeatBgColor;
        }

        if (frameImage != null)
        {
            frameImage.color = isVictory
                ? victoryFrameColor
                : defeatFrameColor;
        }
    }

    // =========================================
    // LOAD HERO VISUALS
    // =========================================

    void LoadHeroVisuals()
    {
        HeroData hero = null;

        if (GameSessionManager.Instance != null)
            hero = GameSessionManager.Instance.selectedHero;

        if (hero == null)
            return;

        if (heroIcon != null && hero.Icon != null)
        {
            heroIcon.sprite = hero.Icon;
        }

        if (heroNameText != null)
            heroNameText.text = hero.heroName;
    }

    // =========================================
    // RESET ALL
    // =========================================

    void ResetAll()
    {
        SetText(timerValueText, "");
        SetText(totalGoldValueText, "");
        SetText(killsValueText, "");
        SetText(healValueText, "");
        SetText(damageValueText, "");

        if (trophyDeltaText != null)
            trophyDeltaText.alpha = 0;

        if (masteryXPText != null)
            masteryXPText.alpha = 0;

        SetBarFill(masteryBarFill, 0);

        UpdateMasteryIcon(data != null ? data.masteryTierBefore : MasteryTier.Unranked);

        if (exitButton != null)
            exitButton.SetActive(false);
    }

    void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    void SetBarFill(Image bar, float value)
    {
        if (bar != null)
            bar.fillAmount = value;
    }

    void UpdateMasteryIcon(MasteryTier tier)
    {
        if (masteryTierIcon == null)
            return;

        if (masteryTierIcons != null)
        {
            Sprite icon = masteryTierIcons.GetIcon(tier);

            if (icon != null)
            {
                masteryTierIcon.sprite = icon;
                masteryTierIcon.enabled = true;
            }
            else
            {
                masteryTierIcon.enabled = false;
            }
        }
        else
        {
            masteryTierIcon.enabled = false;
        }
    }

    // =========================================
    // MAIN SEQUENCE
    // =========================================

    IEnumerator SummarySequence()
    {
        yield return new WaitForSeconds(0.4f);

        if (data == null)
        {
            Debug.LogError("RunSummaryData is NULL");
            ShowExitButton();
            yield break;
        }

        // Victoria: burst de partículas
        if (isVictory)
        {
            yield return StartCoroutine(VictoryBurst());
            yield return new WaitForSeconds(0.3f);
        }

        // Timer
        int timeSeconds = Mathf.FloorToInt(data.timeSurvived);
        yield return StartCoroutine(
            CountUp(timerValueText, 0, timeSeconds, countDuration, "{0}s"));

        yield return new WaitForSeconds(statDelay);

        // Total Gold
        PlayAudio(goldTotalAudio);

        yield return StartCoroutine(
            CountUp(totalGoldValueText, 0, data.totalReward, countDuration * 1.2f, "+{0}"));

        yield return StartCoroutine(PunchScale(totalGoldValueText, 1.25f));

        yield return new WaitForSeconds(statDelay);

        // Bottom stats
        StartCoroutine(CountUp(killsValueText, 0, data.enemiesKilled, 0.6f, "+{0}"));
        StartCoroutine(CountUp(healValueText, 0, Mathf.FloorToInt(data.healthHealed), 0.6f, "+{0}"));
        yield return StartCoroutine(
            CountUp(damageValueText, 0, Mathf.FloorToInt(data.damageDealt), 0.6f, "+{0}"));

        yield return new WaitForSeconds(statDelay);

        // Trophies
        yield return StartCoroutine(AnimateTrophies());

        yield return new WaitForSeconds(0.2f);

        // Mastery
        yield return StartCoroutine(AnimateMastery());

        yield return new WaitForSeconds(0.3f);
        ShowExitButton();
    }

    // =========================================
    // VICTORY BURST
    // =========================================

    IEnumerator VictoryBurst()
    {
        // Partículas
        if (victoryBurstParticles != null)
            victoryBurstParticles.Play();

        // Sonido
        if (victoryBurstAudio != null)
            victoryBurstAudio.Play();

        // Punch en el texto VICTORY
        if (resultText != null)
        {
            Vector3 orig = resultText.transform.localScale;
            resultText.transform.localScale = orig * 1.5f;

            float t = 0f;
            float duration = 0.4f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                float scale = Mathf.Lerp(1.5f, 1f, Mathf.SmoothStep(0, 1, progress));
                resultText.transform.localScale = orig * scale;
                yield return null;
            }

            resultText.transform.localScale = orig;
        }
    }

    // =========================================
    // COUNT UP
    // =========================================

    IEnumerator CountUp(TMP_Text text, int from, int to, float duration, string format = "{0}")
    {
        if (text == null)
            yield break;

        float timer = 0f;
        int current = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, timer / duration);

            int value = Mathf.FloorToInt(Mathf.Lerp(from, to, progress));

            if (value != current)
            {
                current = value;
                text.text = string.Format(format, current);
                PlayAudio(countTickAudio);
            }

            yield return null;
        }

        text.text = string.Format(format, to);
    }

    // =========================================
    // TROPHIES
    // =========================================

    IEnumerator AnimateTrophies()
    {
        if (trophyDeltaText == null || data == null)
            yield break;

        bool gained = data.trophyDelta >= 0;

        trophyDeltaText.text = gained
            ? "+" + data.trophyDelta
            : data.trophyDelta.ToString();

        trophyDeltaText.color = gained
            ? new Color(0.3f, 1f, 0.3f)
            : new Color(1f, 0.3f, 0.3f);

        yield return StartCoroutine(FadeInText(trophyDeltaText, 0.3f));
        yield return StartCoroutine(PunchScale(trophyDeltaText, 1.3f));
    }

    // =========================================
    // MASTERY
    // =========================================

    IEnumerator AnimateMastery()
    {
        if (masteryBarFill == null || data == null)
            yield break;

        if (masteryTierText != null)
            masteryTierText.text =
                HeroData.GetTierDisplayName(data.masteryTierBefore);

        UpdateMasteryIcon(data.masteryTierBefore);

        float startProgress =
            HeroData.GetTierProgress(
                data.masteryXPTotal - data.masteryXPGained);

        float endProgress =
            HeroData.GetTierProgress(data.masteryXPTotal);

        if (data.masteryTierAfter != data.masteryTierBefore)
        {
            yield return StartCoroutine(
                AnimateBar(masteryBarFill, startProgress, 1f, 0.6f));

            PlayAudio(tierUpAudio);

            UpdateMasteryIcon(data.masteryTierAfter);

            if (masteryTierIcon != null)
                yield return StartCoroutine(PunchScale(masteryTierIcon, 1.4f));

            if (masteryTierText != null)
            {
                masteryTierText.text =
                    HeroData.GetTierDisplayName(data.masteryTierAfter);

                yield return StartCoroutine(PunchScale(masteryTierText, 1.3f));
            }

            masteryBarFill.fillAmount = 0f;

            yield return StartCoroutine(
                AnimateBar(masteryBarFill, 0f, endProgress, 0.5f));
        }
        else
        {
            yield return StartCoroutine(
                AnimateBar(masteryBarFill, startProgress, endProgress, 0.8f));
        }

        if (masteryXPText != null)
        {
            masteryXPText.text = "(+" + data.masteryXPGained + ")";
            yield return StartCoroutine(FadeInText(masteryXPText, 0.3f));
            yield return StartCoroutine(PunchScale(masteryXPText, 1.2f));

            yield return new WaitForSeconds(0.8f);

            int currentXP = data.masteryXPTotal;
            MasteryTier currentTier = data.masteryTierAfter;

            int nextTierIndex = (int)currentTier + 1;
            int nextTierXP;

            if (currentTier == MasteryTier.Master)
            {
                nextTierXP = HeroData.GetXPForTier(MasteryTier.Master);
            }
            else
            {
                nextTierXP = HeroData.GetXPForTier((MasteryTier)nextTierIndex);
            }

            yield return StartCoroutine(FadeOutText(masteryXPText, 0.15f));

            masteryXPText.text = currentXP + " / " + nextTierXP;
            yield return StartCoroutine(FadeInText(masteryXPText, 0.3f));
        }
    }

    IEnumerator AnimateBar(Image bar, float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            bar.fillAmount = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, t / duration));
            yield return null;
        }

        bar.fillAmount = to;
    }

    // =========================================
    // UTILITIES
    // =========================================

    IEnumerator PunchScale(Component target, float punchSize)
    {
        if (target == null)
            yield break;

        Vector3 orig = target.transform.localScale;
        target.transform.localScale = orig * punchSize;
        yield return new WaitForSeconds(0.08f);
        target.transform.localScale = orig;
    }

    IEnumerator FadeInText(TMP_Text text, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = t / duration;
            yield return null;
        }

        text.alpha = 1f;
    }

    IEnumerator FadeOutText(TMP_Text text, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = 1f - (t / duration);
            yield return null;
        }

        text.alpha = 0f;
    }

    void PlayAudio(AudioSource source)
    {
        if (source != null)
            source.Play();
    }

    // =========================================
    // EXIT BUTTON
    // =========================================

    void ShowExitButton()
    {
        if (exitButton == null)
            return;

        exitButton.SetActive(true);

        if (exitButtonCanvasGroup != null)
        {
            exitButtonCanvasGroup.interactable = true;
            exitButtonCanvasGroup.blocksRaycasts = true;
        }

        StartCoroutine(FadeInButton());
    }

    IEnumerator FadeInButton()
    {
        if (exitButtonCanvasGroup == null)
            yield break;

        exitButtonCanvasGroup.alpha = 0;
        exitButtonCanvasGroup.interactable = false;

        float t = 0f;

        while (t < 0.3f)
        {
            t += Time.deltaTime;
            exitButtonCanvasGroup.alpha = t / 0.3f;
            yield return null;
        }

        exitButtonCanvasGroup.alpha = 1f;
        exitButtonCanvasGroup.interactable = true;
        exitButtonCanvasGroup.blocksRaycasts = true;
    }

    // =========================================
    // BUTTON CALLBACK
    // =========================================

    public void OnExitButton()
    {
        Debug.Log("EXIT button pressed");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToHub();
        }
        else
        {
            Debug.LogError("GameManager.Instance is NULL");
            UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
        }
    }
}