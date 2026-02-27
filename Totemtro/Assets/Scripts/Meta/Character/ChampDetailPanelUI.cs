using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ChampDetailPanelUI : MonoBehaviour
{
    public static ChampDetailPanelUI Instance;

    [Header("Root")]
    public GameObject root;

    [Header("Top Info")]
    public TMP_Text nameText;

    [Header("Level")]
    public TMP_Text levelText;
    public Image levelBackground;

    [Header("Character")]
    public Image characterImage;

    [Header("Description")]
    public TMP_Text descriptionText;

    [Header("Stats Bars")]
    public Image healthBarFill;
    public Image damageBarFill;
    public Image speedBarFill;
    public Image fireRateBarFill;

    [Header("Stats Text")]
    public TMP_Text healthValueText;
    public TMP_Text damageValueText;
    public TMP_Text speedValueText;
    public TMP_Text fireRateValueText;

    [Header("Increase Texts")]
    public TMP_Text healthIncreaseText;
    public TMP_Text damageIncreaseText;
    public TMP_Text speedIncreaseText;
    public TMP_Text fireRateIncreaseText;

    [Header("Visual Max Values")]
    public float maxHealthVisual = 300f;
    public float maxDamageVisual = 100f;
    public float maxSpeedVisual = 15f;
    public float maxFireRateVisual = 5f;

    [Header("Unlock / Upgrade UI")]
    public HeroUnlockAndUpgradeUI unlockUpgradeUI;

    HeroData currentHero;

    const float maxStatVisual = 100f; // normalizador visual de barras

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    void OnEnable()
    {
        HeroUnlockAndUpgradeUI.OnHeroUpgraded += Refresh;
    }

    void OnDisable()
    {
        HeroUnlockAndUpgradeUI.OnHeroUpgraded -= Refresh;
    }

    // =========================================
    // OPEN PANEL
    // =========================================

    public void Open(HeroData hero)
    {
        if (hero == null)
            return;

        currentHero = hero;
        root.SetActive(true);

        // 🔹 Nombre y clase
        nameText.text = hero.heroName;

        // 🔹 Imagen principal
        if (hero.Icon != null)
        {
            characterImage.sprite = hero.Icon;
        }

        // 🔹 Nivel
        int level =
            HeroProgressSystem.Instance.GetLevel(hero.heroType);

        levelText.text = level.ToString();

        if (HeroProgressSystem.Instance.IsMaxLevel(hero.heroType))
            levelBackground.color = Color.red;
        else
        {
            Color cyan;
            ColorUtility.TryParseHtmlString("#00FFF3", out cyan);
            levelBackground.color = cyan;
        }

        // 🔹 Descripción
        descriptionText.text = hero.description;

        // 🔹 Stats
        float scaledHealth =
    HeroProgressSystem.Instance.GetScaledHealth(hero);

        float scaledDamage =
            HeroProgressSystem.Instance.GetScaledDamage(hero);

        float scaledSpeed =
            HeroProgressSystem.Instance.GetScaledSpeed(hero);

        float scaledFireRate =
            HeroProgressSystem.Instance.GetScaledFireRate(hero);

        SetBar(healthBarFill, scaledHealth, maxHealthVisual);
        SetBar(damageBarFill, scaledDamage, maxDamageVisual);
        SetBar(speedBarFill, scaledSpeed, maxSpeedVisual);
        SetBar(fireRateBarFill, scaledFireRate, maxFireRateVisual);

        healthValueText.text = scaledHealth.ToString("0");
        damageValueText.text = scaledDamage.ToString("0");
        speedValueText.text = scaledSpeed.ToString("0.0");
        fireRateValueText.text = scaledFireRate.ToString("0.00");

        // 🔹 Botones dinámicos
        if (unlockUpgradeUI != null)
            unlockUpgradeUI.Setup(hero);

        UpdateIncreaseTexts(hero);
    }

    // =========================================
    // REFRESH AFTER UPGRADE
    // =========================================

    void Refresh(HeroData hero)
    {
        if (currentHero == hero)
        {
            StartCoroutine(LevelUpAnimation());
            Open(hero);
        }
    }

    // =========================================
    // UTIL
    // =========================================

    void SetBar(Image fill, float value, float maxVisual)
    {
        if (fill == null)
            return;

        float target = Mathf.Clamp01(value / maxVisual);
        StartCoroutine(AnimateBar(fill, target));
    }

    // =========================================
    // CLOSE
    // =========================================

    public void Close()
    {
        root.SetActive(false);
    }

    // =========================================
    // SELECT HERO
    // =========================================

    public void SelectHero()
    {
        if (currentHero == null)
            return;

        HeroSelectionManager.Instance.SelectHero(currentHero);
        Close();
    }

    void UpdateIncreaseTexts(HeroData hero)
    {
        if (HeroProgressSystem.Instance.IsMaxLevel(hero.heroType))
        {
            ClearIncreaseTexts();
            return;
        }

        int level = HeroProgressSystem.Instance.GetLevel(hero.heroType);
        int nextLevel = level + 1;

        float currentHealth =
            HeroProgressSystem.Instance.GetScaledHealth(hero);

        float nextHealth =
            hero.maxHealth * hero.healthScaling.Evaluate(nextLevel);

        float currentDamage =
            HeroProgressSystem.Instance.GetScaledDamage(hero);

        float nextDamage =
            hero.damage * hero.damageScaling.Evaluate(nextLevel);

        float currentSpeed =
            HeroProgressSystem.Instance.GetScaledSpeed(hero);

        float nextSpeed =
            hero.moveSpeed * hero.speedScaling.Evaluate(nextLevel);

        float currentFire =
            HeroProgressSystem.Instance.GetScaledFireRate(hero);

        float nextFire =
            hero.fireRate * hero.fireRateScaling.Evaluate(nextLevel);

        SetIncreaseText(healthIncreaseText, nextHealth - currentHealth);
        SetIncreaseText(damageIncreaseText, nextDamage - currentDamage);
        SetIncreaseText(speedIncreaseText, nextSpeed - currentSpeed, "0.0");
        SetIncreaseText(fireRateIncreaseText, nextFire - currentFire, "0.00");
    }

    void SetIncreaseText(TMP_Text text, float value, string format = "0")
    {
        if (value <= 0)
        {
            text.text = "";
            return;
        }

        text.text = "(+" + value.ToString(format) + ")";
        text.color = Color.green;
    }

    void ClearIncreaseTexts()
    {
        healthIncreaseText.text = "";
        damageIncreaseText.text = "";
        speedIncreaseText.text = "";
        fireRateIncreaseText.text = "";
    }

    IEnumerator LevelUpAnimation()
    {
        Vector3 originalScale = levelText.transform.localScale;

        levelText.color = Color.green;
        levelText.transform.localScale = originalScale * 1.4f;

        yield return new WaitForSeconds(0.25f);

        levelText.transform.localScale = originalScale;
        levelText.color = Color.white;
    }

    IEnumerator AnimateBar(Image fill, float target)
    {
        float start = fill.fillAmount;
        float timer = 0f;
        float duration = 0.3f;

        while (timer < duration)
        {
            fill.fillAmount =
                Mathf.Lerp(start, target, timer / duration);

            timer += Time.deltaTime;
            yield return null;
        }

        fill.fillAmount = target;
    }
}