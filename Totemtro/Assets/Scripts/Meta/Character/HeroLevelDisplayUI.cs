using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroLevelDisplayUI : MonoBehaviour
{
    public Image backgroundImage;
    public TMP_Text levelText;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (HeroSelectionSystem.Instance == null)
            return;

        var hero = HeroSelectionSystem.Instance.selectedHero;
        if (hero == null)
            return;

        int level =
            HeroProgressSystem.Instance.GetLevel(hero.heroType);

        levelText.text = level.ToString();

        if (HeroProgressSystem.Instance.IsMaxLevel(hero.heroType))
        {
            backgroundImage.color = Color.red;
        }
        else
        {
            Color cyan;
            ColorUtility.TryParseHtmlString("#00FFF3", out cyan);
            backgroundImage.color = cyan;
        }
    }
}