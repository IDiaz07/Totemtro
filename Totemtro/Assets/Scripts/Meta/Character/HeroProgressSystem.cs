using UnityEngine;
using System.Collections.Generic;

public class HeroProgressSystem : MonoBehaviour
{
    public static HeroProgressSystem Instance;
    public const int MAX_LEVEL = 9;
    Dictionary<HeroType, int> heroLevels = new();
    Dictionary<HeroType, bool> heroUnlocked = new();

    Dictionary<HeroType, int> heroFragments = new();

    const int BASE_FRAGMENT_COST = 10;
    const int BASE_GOLD_COST = 100;

    const int defaultLevel = 1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // =========================================
    // INIT
    // =========================================

    public void Initialize(List<HeroData> allHeroes)
    {
        foreach (var hero in allHeroes)
        {
            LoadHero(hero);
        }
    }



    void SaveHero(HeroType type)
    {
        PlayerPrefs.SetInt($"Hero_{type}_Level", heroLevels[type]);
        PlayerPrefs.SetInt($"Hero_{type}_Unlocked", heroUnlocked[type] ? 1 : 0);
        PlayerPrefs.Save();
    }

    // =========================================
    // GETTERS
    // =========================================

    public int GetLevel(HeroType type)
    {
        return heroLevels.ContainsKey(type) ? heroLevels[type] : defaultLevel;
    }

    public bool IsUnlocked(HeroType type)
    {
        return heroUnlocked.ContainsKey(type) && heroUnlocked[type];
    }

    // =========================================
    // UNLOCK
    // =========================================

    public bool TryUnlock(HeroData hero)
    {
        if (IsUnlocked(hero.heroType))
            return true;

        if (MetaCurrencySystem.Instance == null)
            return false;

        if (!MetaCurrencySystem.Instance.Spend(hero.gemCost))
            return false;

        heroUnlocked[hero.heroType] = true;
        SaveHero(hero.heroType);

        return true;
    }

    // =========================================
    // LEVEL UP (para futuro)
    // =========================================

    public void AddLevel(HeroType type, int amount)
    {
        if (!heroLevels.ContainsKey(type))
            heroLevels[type] = defaultLevel;

        heroLevels[type] += amount;

        if (heroLevels[type] > MAX_LEVEL)
            heroLevels[type] = MAX_LEVEL;

        SaveHero(type);
    }

    public bool IsMaxLevel(HeroType type)
    {
        return GetLevel(type) >= MAX_LEVEL;
    }

    void LoadHero(HeroData hero)
    {
        string levelKey = $"Hero_{hero.heroType}_Level";
        string unlockKey = $"Hero_{hero.heroType}_Unlocked";
        string fragKey = $"Hero_{hero.heroType}_Fragments";

        int level = PlayerPrefs.GetInt(levelKey, 1);
        bool unlocked =
            hero.unlockedByDefault ||
            PlayerPrefs.GetInt(unlockKey, hero.unlockedByDefault ? 1 : 0) == 1;

        int fragments = PlayerPrefs.GetInt(fragKey, 0);

        heroLevels[hero.heroType] = level;
        heroUnlocked[hero.heroType] = unlocked;
        heroFragments[hero.heroType] = fragments;
    }

    public int GetFragments(HeroType type)
    {
        return heroFragments.ContainsKey(type) ? heroFragments[type] : 0;
    }

    public void AddFragments(HeroType type, int amount)
    {
        heroFragments[type] += amount;
        PlayerPrefs.SetInt($"Hero_{type}_Fragments", heroFragments[type]);
    }

    public bool CanUpgrade(HeroType type)
    {
        if (IsMaxLevel(type))
            return false;

        int level = GetLevel(type);

        int requiredFragments = BASE_FRAGMENT_COST * level;
        int requiredGold = BASE_GOLD_COST * level;

        return GetFragments(type) >= requiredFragments &&
               MetaCurrencySystem.Instance.MetaGold >= requiredGold;
    }

    public bool Upgrade(HeroType type)
    {
        if (!CanUpgrade(type))
            return false;

        int level = GetLevel(type);

        int requiredFragments = BASE_FRAGMENT_COST * level;
        int requiredGold = BASE_GOLD_COST * level;

        heroFragments[type] -= requiredFragments;
        MetaCurrencySystem.Instance.Spend(requiredGold);

        AddLevel(type, 1);

        PlayerPrefs.SetInt($"Hero_{type}_Fragments", heroFragments[type]);

        return true;
    }

    public float GetScaledHealth(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        float multiplier = hero.healthScaling.Evaluate(level);
        return hero.maxHealth * multiplier;
    }

    public float GetScaledDamage(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        float multiplier = hero.damageScaling.Evaluate(level);
        return hero.damage * multiplier;
    }

    public float GetScaledSpeed(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        float multiplier = hero.speedScaling.Evaluate(level);
        return hero.moveSpeed * multiplier;
    }

    public float GetScaledFireRate(HeroData hero)
    {
        int level = GetLevel(hero.heroType);
        float multiplier = hero.fireRateScaling.Evaluate(level);
        return hero.fireRate * multiplier;
    }
}