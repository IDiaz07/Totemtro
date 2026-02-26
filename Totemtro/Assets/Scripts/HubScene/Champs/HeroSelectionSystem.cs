using UnityEngine;
using System;

public class HeroSelectionSystem : MonoBehaviour
{
    public static HeroSelectionSystem Instance;

    public HeroData selectedHero;

    public Action OnHeroSelected;

    void Awake()
    {
        Instance = this;
    }

    public void SelectHero(HeroData hero)
    {
        selectedHero = hero;
        OnHeroSelected?.Invoke();
    }
}