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

    [Header("Default Hero (Tro)")]
    public HeroData defaultHero;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadHero();
    }

    // =================================
    // SELECT HERO
    // =================================

    public void SelectHero(HeroData hero)
    {
        SelectedHero = hero;

        PlayerPrefs.SetInt(HERO_PREF_KEY, (int)hero.heroType);
        PlayerPrefs.Save();

        OnHeroChanged?.Invoke(hero);
    }

    // =================================
    // LOAD HERO
    // =================================

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
                SelectedHero = hero;
                return;
            }
        }

        SelectedHero = defaultHero;
    }
}