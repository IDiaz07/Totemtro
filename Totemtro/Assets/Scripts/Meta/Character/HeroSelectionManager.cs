using UnityEngine;
using System;

public class HeroSelectionManager : MonoBehaviour
{
    public static HeroSelectionManager Instance;

    public HeroData SelectedHero { get; private set; }

    public static Action<HeroData> OnHeroChanged;

    const string HERO_PREF_KEY = "SelectedHero";

    [Header("All Heroes")]
    public HeroData[] allHeroes;

    [Header("Default Hero")]
    public HeroData defaultHero;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadHero();
        OnHeroChanged?.Invoke(SelectedHero);
    }

    public void SelectHero(HeroData hero)
    {
        if (hero == null)
            return;

        if (HeroProgressSystem.Instance != null &&
            !HeroProgressSystem.Instance.IsUnlocked(hero.heroType))
        {
            Debug.Log("Hero locked: " + hero.heroName);
            return;
        }

        SelectedHero = hero;

        PlayerPrefs.SetInt(HERO_PREF_KEY, (int)hero.heroType);
        PlayerPrefs.Save();

        Debug.Log("Hero selected: " + hero.heroName);

        OnHeroChanged?.Invoke(hero);
    }

    void LoadHero()
    {
        if (!PlayerPrefs.HasKey(HERO_PREF_KEY))
        {
            SelectedHero = defaultHero;
            return;
        }

        HeroType savedType =
            (HeroType)PlayerPrefs.GetInt(HERO_PREF_KEY);

        foreach (var hero in allHeroes)
        {
            if (hero.heroType == savedType)
            {
                if (HeroProgressSystem.Instance == null ||
                    HeroProgressSystem.Instance.IsUnlocked(hero.heroType))
                {
                    SelectedHero = hero;
                    return;
                }
            }
        }

        SelectedHero = defaultHero;
    }
}
