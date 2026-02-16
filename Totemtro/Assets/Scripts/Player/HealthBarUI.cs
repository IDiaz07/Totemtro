using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public HeroController hero;

    void Start()
    {
        hero.OnHealthChanged += UpdateBar;
        UpdateBar();
    }

    void UpdateBar()
    {
        fillImage.fillAmount =
            hero.CurrentHealth / hero.MaxHealth;
    }

}
