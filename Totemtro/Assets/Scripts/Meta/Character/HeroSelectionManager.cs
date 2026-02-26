using UnityEngine;
using System;

public class HeroSelectionManager : MonoBehaviour
{
    public static HeroSelectionManager Instance;

    public HeroData SelectedHero { get; private set; }

    public static Action<HeroData> OnHeroChanged;

    void Awake()
    {
        Instance = this;
    }

    public void SelectHero(HeroData hero)
    {
        SelectedHero = hero;
        PlayerPrefs.SetInt("SelectedHero", (int)hero.heroType);
        OnHeroChanged?.Invoke(hero);
    }
}