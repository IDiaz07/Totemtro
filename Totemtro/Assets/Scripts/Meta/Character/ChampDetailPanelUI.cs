using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        if (hero.directionalSprites != null &&
            hero.directionalSprites.FrontView != null)
        {
            characterImage.sprite =
                hero.directionalSprites.FrontView;
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

        SetBar(healthBarFill, scaledHealth);
        SetBar(damageBarFill, scaledDamage);
        SetBar(speedBarFill, scaledSpeed);
        SetBar(fireRateBarFill, scaledFireRate);

        healthValueText.text = scaledHealth.ToString("0");
        damageValueText.text = scaledDamage.ToString("0");
        speedValueText.text = scaledSpeed.ToString("0.0");
        fireRateValueText.text = scaledFireRate.ToString("0.00");

        // 🔹 Botones dinámicos
        if (unlockUpgradeUI != null)
            unlockUpgradeUI.Setup(hero);
    }

    // =========================================
    // REFRESH AFTER UPGRADE
    // =========================================

    void Refresh(HeroData hero)
    {
        if (currentHero == hero)
            Open(hero);
    }

    // =========================================
    // UTIL
    // =========================================

    void SetBar(Image fill, float value)
    {
        if (fill == null)
            return;

        float normalized = Mathf.Clamp01(value / maxStatVisual);
        fill.fillAmount = normalized;
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
}