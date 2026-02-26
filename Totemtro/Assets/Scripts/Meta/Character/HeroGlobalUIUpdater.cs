using UnityEngine;
using UnityEngine.UI;

public class HeroGlobalUIUpdater : MonoBehaviour
{
    public Image[] heroIcons;

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
        foreach (var img in heroIcons)
        {
            img.sprite = hero.Icon;
        }
    }
}