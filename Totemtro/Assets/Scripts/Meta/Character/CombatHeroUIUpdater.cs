using UnityEngine;
using UnityEngine.UI;

public class CombatHeroUIUpdater : MonoBehaviour
{
    public Image heroIcon;

    void Start()
    {
        if (HeroSelectionManager.Instance == null)
            return;

        HeroData hero = HeroSelectionManager.Instance.SelectedHero;

        if (hero != null && heroIcon != null)
        {
            heroIcon.sprite = hero.Icon;
        }
    }
}
