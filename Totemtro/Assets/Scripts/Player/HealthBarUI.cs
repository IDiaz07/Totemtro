using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public HeroController hero;
    public TMP_Text healthText;

    void Start()
    {
        hero.OnHealthChanged += UpdateBar;
        UpdateBar();
    }

    void UpdateBar()
    {
        if (hero == null) return;

        float percent =
            hero.CurrentHealth / hero.MaxHealth;

        fillImage.fillAmount = percent;

        healthText.text =
            Mathf.CeilToInt(hero.CurrentHealth) +
            " / " +
            Mathf.CeilToInt(hero.MaxHealth);
    }
}
