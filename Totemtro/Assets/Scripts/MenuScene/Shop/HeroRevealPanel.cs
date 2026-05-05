using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroRevealPanel : MonoBehaviour
{
    public static HeroRevealPanel Instance;

    public GameObject panel;
    public Image heroImage;
    public TMP_Text heroNameText;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        HeroProgressSystem.OnHeroUnlocked += PlayReveal;
    }

    void OnDisable()
    {
        HeroProgressSystem.OnHeroUnlocked -= PlayReveal;
    }

    public void PlayReveal(HeroData hero)
    {
        if (hero == null)
            return;

        panel.SetActive(true);

        heroImage.sprite = hero.ChampsHeadIcon;

        if (heroNameText != null)
            heroNameText.text = hero.heroName;
    }

    public void Close()
    {
        panel.SetActive(false);
    }
}