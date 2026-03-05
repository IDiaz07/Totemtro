using UnityEngine;
using System;
using System.Collections.Generic;

public class HeroProgressSystem : MonoBehaviour
{
    public static HeroProgressSystem Instance;

    public const int MAX_LEVEL = 9;

    Dictionary<HeroType, int> heroLevels = new();
    Dictionary<HeroType, bool> heroUnlocked = new();
    Dictionary<HeroType, int> heroFragments = new();

    public static Action<HeroData> OnHeroUnlocked;
    public static Action<HeroType, int> OnFragmentsAdded;

    const int BASE_FRAGMENT_COST = 10;
    const int BASE_GOLD_COST = 100;
    const int defaultLevel = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            HeroProgressSystem.Instance.ResetAllHeroesExcept(HeroType.Tro);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 🔥 CLAVE ABSOLUTA
        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // DATABASE
    // =========================

    List<HeroData> heroDatabase = new();

    public void Initialize(List<HeroData> allHeroes)
    {
        heroDatabase = allHeroes;

        foreach (var hero in allHeroes)
            LoadHero(hero);
    }

    public HeroData GetHeroData(HeroType type)
    {
        return heroDatabase.Find(h => h.heroType == type);
    }

    // =========================
    // BASIC GETTERS
    // =========================

    public int GetLevel(HeroType type)
    {
        return heroLevels.ContainsKey(type) ? heroLevels[type] : defaultLevel;
    }

    public bool IsUnlocked(HeroType type)
    {
        return heroUnlocked.ContainsKey(type) && heroUnlocked[type];
    }

    public int GetFragments(HeroType type)
    {
        return heroFragments.ContainsKey(type) ? heroFragments[type] : 0;
    }

    public bool IsMaxLevel(HeroType type)
    {
        return GetLevel(type) >= MAX_LEVEL;
    }

    // =========================
    // UNLOCK BY RARITY
    // =========================

    public int GetRequiredFragmentsForUnlock(HeroType type)
    {
        HeroData hero = GetHeroData(type);
        if (hero == null)
            return 150;

        switch (hero.rarity)
        {
            case HeroRarity.Rare: return 150;
            case HeroRarity.Epic: return 250;
            case HeroRarity.Legendary: return 350;
        }

        return 150;
    }

    // =========================
    // GEM PRICE (ROUNDED)
    // =========================

    public int GetGemUnlockPrice(HeroType type)
    {
        HeroData hero = GetHeroData(type);
        if (hero == null)
            return 650;

        int basePrice = 650;

        switch (hero.rarity)
        {
            case HeroRarity.Rare: basePrice = 650; break;
            case HeroRarity.Epic: basePrice = 1000; break;
            case HeroRarity.Legendary: basePrice = 1400; break;
        }

        float multiplier = 1f;

        if (PlayerShopProfileSystem.Instance != null)
        {
            if (PlayerShopProfileSystem.Instance.IsWhale())
                multiplier += 0.10f;
            else if (PlayerShopProfileSystem.Instance.IsF2P())
                multiplier -= 0.10f;

            if (!PlayerShopProfileSystem.Instance.HasMadeFirstPurchase())
                multiplier -= 0.20f;
        }

        float finalPrice = basePrice * multiplier;

        int rounded = Mathf.RoundToInt(finalPrice / 50f) * 50;

        return Mathf.Max(50, rounded);
    }

    public int GetLimitedOfferPrice(HeroType type)
    {
        int normalPrice = GetGemUnlockPrice(type);

        // 🔥 FIX CRÍTICO
        if (LimitedHeroOfferSystem.Instance == null)
            return normalPrice;

        if (!LimitedHeroOfferSystem.Instance.IsHeroOnOffer(type))
            return normalPrice;

        int discount =
            LimitedHeroOfferSystem.Instance.GetDiscountPercent();

        float multiplier = 1f - (discount / 100f);

        return RoundPrice(normalPrice * multiplier);
    }

    // =========================
    // TRY UNLOCK
    // =========================

    public bool TryUnlock(HeroData hero, bool chargeCurrency = true)
    {
        if (IsUnlocked(hero.heroType))
            return true;

        if (chargeCurrency)
        {
            int gemCost = GetLimitedOfferPrice(hero.heroType);

            if (!MetaCurrencySystem.Instance.SpendGems(gemCost))
                return false;
        }

        heroUnlocked[hero.heroType] = true;
        SaveHero(hero.heroType);

        OnHeroUnlocked?.Invoke(hero);

        return true;
    }

    // =========================
    // LEVEL UP
    // =========================

    public bool CanUpgrade(HeroType type)
    {
        if (IsMaxLevel(type))
            return false;

        int level = GetLevel(type);
        int requiredFragments = BASE_FRAGMENT_COST * level;
        int requiredGold = BASE_GOLD_COST * level;

        return GetFragments(type) >= requiredFragments &&
               MetaCurrencySystem.Instance.Gold >= requiredGold;
    }

    public bool Upgrade(HeroType type)
    {
        if (!CanUpgrade(type))
            return false;

        int level = GetLevel(type);
        int requiredFragments = BASE_FRAGMENT_COST * level;
        int requiredGold = BASE_GOLD_COST * level;

        heroFragments[type] -= requiredFragments;
        MetaCurrencySystem.Instance.SpendGold(requiredGold);

        AddLevel(type, 1);
        SaveHero(type);

        return true;
    }

    public void AddLevel(HeroType type, int amount)
    {
        if (!heroLevels.ContainsKey(type))
            heroLevels[type] = defaultLevel;

        heroLevels[type] += amount;

        if (heroLevels[type] > MAX_LEVEL)
            heroLevels[type] = MAX_LEVEL;
    }

    // =========================
    // FRAGMENTS
    // =========================

    public void AddFragments(HeroType type, int amount)
    {
        if (!heroFragments.ContainsKey(type))
            heroFragments[type] = 0;

        heroFragments[type] += amount;

        SaveHero(type);

        OnFragmentsAdded?.Invoke(type, amount);

        CheckAutoUnlock(type);
    }

    void CheckAutoUnlock(HeroType type)
    {
        if (IsUnlocked(type))
            return;

        if (GetFragments(type) >= GetRequiredFragmentsForUnlock(type))
        {
            heroUnlocked[type] = true;
            SaveHero(type);

            HeroData hero = GetHeroData(type);
            if (hero != null)
                OnHeroUnlocked?.Invoke(hero);
        }
    }

    // =========================
    // SCALED STATS
    // =========================

    public float GetScaledHealth(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        return hero.maxHealth * hero.healthScaling.Evaluate(level);
    }

    public float GetScaledDamage(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        return hero.damage * hero.damageScaling.Evaluate(level);
    }

    public float GetScaledSpeed(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        return hero.moveSpeed * hero.speedScaling.Evaluate(level);
    }

    public float GetScaledFireRate(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        return hero.fireRate * hero.fireRateScaling.Evaluate(level);
    }

    // =========================
    // SAVE / LOAD
    // =========================

    void SaveHero(HeroType type)
    {
        PlayerPrefs.SetInt($"Hero_{type}_Level", GetLevel(type));
        PlayerPrefs.SetInt($"Hero_{type}_Unlocked", IsUnlocked(type) ? 1 : 0);
        PlayerPrefs.SetInt($"Hero_{type}_Fragments", GetFragments(type));
        PlayerPrefs.Save();
    }

    void LoadHero(HeroData hero)
    {
        heroLevels[hero.heroType] =
            PlayerPrefs.GetInt($"Hero_{hero.heroType}_Level", 1);

        heroUnlocked[hero.heroType] =
            PlayerPrefs.GetInt($"Hero_{hero.heroType}_Unlocked",
                hero.unlockedByDefault ? 1 : 0) == 1;

        heroFragments[hero.heroType] =
            PlayerPrefs.GetInt($"Hero_{hero.heroType}_Fragments", 0);
    }

    public List<HeroData> GetAllHeroes()
    {
        return heroDatabase;
    }

    int RoundPrice(float value)
    {
        int v = Mathf.RoundToInt(value);

        // 🔥 Redondeo psicológico profesional
        if (v >= 1000)
            return Mathf.RoundToInt(v / 50f) * 50;   // múltiplos de 50

        if (v >= 500)
            return Mathf.RoundToInt(v / 25f) * 25;   // múltiplos de 25

        return Mathf.RoundToInt(v / 10f) * 10;       // múltiplos de 10
    }

    // =========================
    // RESET
    // =========================

    public void ResetAllHeroesExcept(HeroType defaultHero)
    {
        foreach (HeroType type in System.Enum.GetValues(typeof(HeroType)))
        {
            heroUnlocked[type] = type == defaultHero;
            heroLevels[type] = 1;
            heroFragments[type] = 0;

            PlayerPrefs.SetInt($"Hero_{type}_Unlocked", type == defaultHero ? 1 : 0);
            PlayerPrefs.SetInt($"Hero_{type}_Level", 1);
            PlayerPrefs.SetInt($"Hero_{type}_Fragments", 0);
        }

        PlayerPrefs.Save();
    }
}