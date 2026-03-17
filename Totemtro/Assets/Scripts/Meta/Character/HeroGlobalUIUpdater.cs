using UnityEngine;
using UnityEngine.UI;

public class HeroGlobalUIUpdater : MonoBehaviour
{
    public Image[] heroIcons;

    void Awake()
    {
        // 🔥 actualizar inmediatamente al cargar escena
        if (HeroSelectionManager.Instance != null)
        {
            HeroData hero = HeroSelectionManager.Instance.SelectedHero;

            if (hero != null)
                UpdateIcons(hero);
        }
    }

    void OnEnable()
    {
        HeroSelectionManager.OnHeroChanged += UpdateIcons;
    }

    void OnDisable()
    {
        HeroSelectionManager.OnHeroChanged -= UpdateIcons;
    }

    void UpdateIcons(HeroData hero)
    {
        if (hero == null)
            return;

        foreach (var img in heroIcons)
        {
            img.sprite = hero.Icon;
        }
    }
}