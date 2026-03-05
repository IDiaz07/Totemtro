using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HeroUnlockPresentationSystem : MonoBehaviour
{
    public static HeroUnlockPresentationSystem Instance;

    [Header("Main")]
    public GameObject root;
    public Image blackScreen;

    [Header("Hero")]
    public Image heroArt;
    [SerializeField] float parallaxAmount = 10f;
    Vector2 heroOriginalPos;

    [Header("Text")]
    public CanvasGroup infoGroup;
    public TMP_Text heroName;
    public TMP_Text rarityText;
    public TMP_Text roleText;
    public TMP_Text descriptionText;
    public Button continueButton;

    [Header("Background Tint")]
    [SerializeField] Image backgroundTint;

    [Header("Stats Bars")]
    [SerializeField] Image healthFill;
    [SerializeField] Image damageFill;
    [SerializeField] Image speedFill;
    [SerializeField] Image fireRateFill;

    [Header("Stat Numbers")]
    [SerializeField] TMP_Text healthValueText;
    [SerializeField] TMP_Text damageValueText;
    [SerializeField] TMP_Text speedValueText;
    [SerializeField] TMP_Text fireRateValueText;

    [Header("Rarity FX")]
    [SerializeField] Image rarityGlow;
    [SerializeField] Image runeCircle;
    [SerializeField] Image waveCircle;
    [SerializeField] ParticleSystem rarityParticles;

    [Header("Glow Sprites")]
    [SerializeField] Sprite glowRare;
    [SerializeField] Sprite glowEpic;
    [SerializeField] Sprite glowLegendary;

    Coroutine activeFXRoutine;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
        heroOriginalPos = heroArt.rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (!root.activeSelf) return;

        Vector2 mouse = Input.mousePosition;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = (mouse - screenCenter) / screenCenter;

        heroArt.rectTransform.anchoredPosition =
            heroOriginalPos + offset * parallaxAmount;
    }

    void OnEnable()
    {
        HeroProgressSystem.OnHeroUnlocked += Play;
    }

    void OnDisable()
    {
        HeroProgressSystem.OnHeroUnlocked -= Play;
    }

    void Play(HeroData hero)
    {
        StartCoroutine(Sequence(hero));
    }

    IEnumerator Sequence(HeroData hero)
    {
        root.SetActive(true);
        continueButton.gameObject.SetActive(false);

        ResetFX();

        // -------- SET DATA --------
        heroArt.sprite = hero.Icon;
        heroName.text = hero.heroName.ToUpper();
        rarityText.text = hero.rarity.ToString().ToUpper();
        roleText.text = hero.role.ToString().ToUpper();
        descriptionText.text = hero.description;

        SetRarityTextColor(hero.rarity);
        SetupRarityFX(hero.rarity);
        SetBackgroundTint(hero.rarity);

        // -------- FASE 1: NEGRO --------
        blackScreen.color = Color.black;
        heroArt.transform.localScale = Vector3.zero;
        infoGroup.alpha = 0;

        yield return new WaitForSeconds(0.4f);

        float t = 0;
        while (t < 0.6f)
        {
            float pulse = Mathf.Sin(t * 12f) * 0.05f;
            blackScreen.color = new Color(0, 0, 0, 1f - pulse);
            t += Time.deltaTime;
            yield return null;
        }

        // -------- FASE 2: REVEAL --------
        if (hero.rarity == HeroRarity.Legendary)
            StartCoroutine(LegendarySlowMotion());

        t = 0;
        while (t < 0.5f)
        {
            float scale = Mathf.Lerp(0, 1.15f, t / 0.5f);
            heroArt.transform.localScale = Vector3.one * scale;
            t += Time.deltaTime;
            yield return null;
        }

        heroArt.transform.localScale = Vector3.one;
        blackScreen.CrossFadeAlpha(0f, 0.3f, false);

        yield return new WaitForSeconds(0.2f);

        // -------- FASE 3: INFO --------
        infoGroup.alpha = 1;

        yield return StartCoroutine(PopText(heroName.transform, 0.25f));
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(PopText(roleText.transform, 0.2f));
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(PopText(rarityText.transform, 0.2f));

        // -------- STATS --------
        yield return StartCoroutine(
            AnimateStat(healthFill, healthValueText, hero.maxHealth, 500f, 0.6f));

        yield return StartCoroutine(
            AnimateStat(damageFill, damageValueText, hero.damage, 200f, 0.6f));

        yield return StartCoroutine(
            AnimateStat(speedFill, speedValueText, hero.moveSpeed, 15f, 0.6f));

        yield return StartCoroutine(
            AnimateStat(fireRateFill, fireRateValueText, hero.fireRate, 10f, 0.6f));

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(Close);
    }

    void Close()
    {
        ResetFX();
        root.SetActive(false);
    }

    void ResetFX()
    {
        rarityGlow.gameObject.SetActive(false);
        runeCircle.gameObject.SetActive(false);
        waveCircle.gameObject.SetActive(false);

        rarityGlow.transform.localScale = Vector3.one;
        runeCircle.transform.localRotation = Quaternion.identity;

        if (rarityParticles != null)
            rarityParticles.Stop();

        if (activeFXRoutine != null)
            StopCoroutine(activeFXRoutine);

        backgroundTint.color = new Color(0, 0, 0, 0);
    }

    void SetRarityTextColor(HeroRarity rarity)
    {
        switch (rarity)
        {
            case HeroRarity.Rare:
                rarityText.color = Color.cyan;
                break;
            case HeroRarity.Epic:
                rarityText.color = new Color(0.7f, 0.2f, 1f);
                break;
            case HeroRarity.Legendary:
                rarityText.color = Color.yellow;
                break;
        }
    }

    void SetupRarityFX(HeroRarity rarity)
    {
        var main = rarityParticles.main;
        rarityGlow.gameObject.SetActive(true);

        switch (rarity)
        {
            case HeroRarity.Rare:
                rarityGlow.sprite = glowRare;
                rarityGlow.color = new Color(1, 1, 1, 0.01f);
                main.startColor = new Color(0f, 1f, 1f, 0.01f);
                rarityParticles.Play();
                activeFXRoutine = StartCoroutine(RarePulse());
                break;

            case HeroRarity.Epic:
                rarityGlow.sprite = glowEpic;
                rarityGlow.color = new Color(1, 1, 1, 0.01f);
                main.startColor = new Color(0.6f, 0.2f, 1f, 0.02f);
                rarityParticles.Play();
                activeFXRoutine = StartCoroutine(EpicGlow());
                break;

            case HeroRarity.Legendary:
                rarityGlow.sprite = glowLegendary;
                rarityGlow.color = new Color(1, 1, 1, 0.01f);
                main.startColor = new Color(1f, 0.8f, 0.2f, 0.01f);
                rarityParticles.Play();
                runeCircle.gameObject.SetActive(true);
                activeFXRoutine = StartCoroutine(LegendaryFXLoop());
                break;
        }

        rarityGlow.rectTransform.localScale = Vector3.one * 1.2f;
    }

    IEnumerator RarePulse()
    {
        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.05f;
            rarityGlow.transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    IEnumerator EpicGlow()
    {
        while (true)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            rarityGlow.color = Color.Lerp(
                new Color(1, 1, 1, 0.01f),
                new Color(1, 1, 1, 0.04f),
                pulse);
            float pulse2 = 1f + Mathf.Sin(Time.time * 3f) * 0.08f;
            rarityGlow.transform.localScale = Vector3.one * pulse2;
            yield return null;
        }
    }

    IEnumerator LegendaryFXLoop()
    {
        while (true)
        {
            runeCircle.transform.Rotate(0, 0, 20f * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.08f;
            rarityGlow.transform.localScale = Vector3.one * pulse;
            yield return null;
        }
    }

    IEnumerator LegendarySlowMotion()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = 1f;
    }

    IEnumerator PopText(Transform target, float duration)
    {
        target.localScale = Vector3.zero;
        float t = 0;
        while (t < duration)
        {
            float scale = Mathf.Lerp(0, 1.2f, t / duration);
            target.localScale = Vector3.one * scale;
            t += Time.deltaTime;
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    IEnumerator AnimateStat(Image fill, TMP_Text valueText, float finalValue, float maxValue, float duration)
    {
        float t = 0;
        fill.fillAmount = 0;

        while (t < duration)
        {
            float progress = t / duration;

            float currentValue = Mathf.Lerp(0, finalValue, progress);
            fill.fillAmount = currentValue / maxValue;

            valueText.text = Mathf.RoundToInt(currentValue).ToString();

            t += Time.deltaTime;
            yield return null;
        }

        fill.fillAmount = finalValue / maxValue;
        valueText.text = Mathf.RoundToInt(finalValue).ToString();
    }

    void SetBackgroundTint(HeroRarity rarity)
    {
        Color targetColor = Color.clear;

        switch (rarity)
        {
            case HeroRarity.Rare:
                targetColor = new Color(0f, 1f, 1f, 0.03f); // Cyan
                break;

            case HeroRarity.Epic:
                targetColor = new Color(0.6f, 0.2f, 1f, 0.03f); // Morado
                break;

            case HeroRarity.Legendary:
                targetColor = new Color(1f, 0.8f, 0.2f, 0.03f); // Dorado
                break;
        }

        StartCoroutine(AnimateBackgroundTint(targetColor));
    }

    IEnumerator AnimateBackgroundTint(Color target)
    {
        Color start = backgroundTint.color;
        float duration = 0.5f;
        float t = 0;

        while (t < duration)
        {
            backgroundTint.color = Color.Lerp(start, target, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        backgroundTint.color = target;
    }
}