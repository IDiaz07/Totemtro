using UnityEngine;
using UnityEngine.UI;

public class HeroVisualBinder : MonoBehaviour
{
    [Header("PLAYER")]
    public HeroController heroController;

    [Header("MINIMAP")]
    public Image minimapPlayerIcon;

    [Header("WORLD MAP")]
    public SpriteRenderer worldMapPlayerIcon;

    [Header("UI ICON (ARMOR / HUD)")]
    public Image heroUIIcon;

    void Start()
    {
        ApplyHero();
    }

    void OnEnable()
    {
        HeroSelectionManager.OnHeroChanged += ApplyHero;
    }

    void OnDisable()
    {
        HeroSelectionManager.OnHeroChanged -= ApplyHero;
    }

    void ApplyHero(HeroData hero = null)
    {
        if (hero == null)
            hero = HeroSelectionManager.Instance?.SelectedHero;

        if (hero == null)
        {
            Debug.LogWarning("No hero selected");
            return;
        }

        // =========================
        // PLAYER (SPRITES DIRECCIONALES)
        // =========================
        if (heroController != null)
        {
            heroController.currentHero = hero;
            heroController.ApplyHero();
        }

        // =========================
        // MINIMAP ICON
        // =========================
        if (minimapPlayerIcon != null && hero.ChampsHeadIcon != null)
        {
            minimapPlayerIcon.sprite = hero.ChampsHeadIcon;
        }

        // =========================
        // WORLD MAP ICON (SpriteRenderer)
        // =========================
        if (worldMapPlayerIcon != null && hero.ChampsHeadIcon != null)
        {
            worldMapPlayerIcon.sprite = hero.ChampsHeadIcon;
        }

        // =========================
        // UI ICON (ARMOR / HUD)
        // =========================
        if (heroUIIcon != null && hero.Icon != null)
        {
            heroUIIcon.sprite = hero.Icon;
        }

        Debug.Log("Hero visual aplicado: " + hero.heroName);
    }
}