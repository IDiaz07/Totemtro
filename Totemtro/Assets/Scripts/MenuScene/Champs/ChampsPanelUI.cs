using UnityEngine;
using System.Collections.Generic;

public class ChampsPanelUI : MonoBehaviour
{
    public Transform contentParent;
    public GameObject heroCardPrefab;

    public HeroData[] allHeroes;

    void Start()
    {
        HeroProgressSystem.Instance.Initialize(new List<HeroData>(allHeroes));
        Generate();
    }

    void OnEnable()
    {
        if (HeroSelectionSystem.Instance != null)
            HeroSelectionSystem.Instance.OnHeroSelected += RefreshSelection;
    }

    void OnDisable()
    {
        if (HeroSelectionSystem.Instance != null)
            HeroSelectionSystem.Instance.OnHeroSelected -= RefreshSelection;
    }

    void Generate()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var hero in allHeroes)
        {
            GameObject card = Instantiate(heroCardPrefab, contentParent);

            var ui = card.GetComponent<HeroCardUI>();

            bool selected =
                HeroSelectionSystem.Instance != null &&
                HeroSelectionSystem.Instance.selectedHero == hero;

            ui.Setup(hero, selected);
        }
    }

    void RefreshSelection()
    {
        Generate();
    }
}