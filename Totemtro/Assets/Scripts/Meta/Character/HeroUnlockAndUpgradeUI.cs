using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroUnlockAndUpgradeUI : MonoBehaviour
{
    public static System.Action<HeroData> OnHeroUpgraded;

    public Button unlockButton;
    public Button selectButton;
    public Button upgradeButton;

    public TMP_Text fragmentCostText;
    public TMP_Text goldCostText;

    [Header("Unlock Cost")]
    public TMP_Text gemCostText;

    HeroData currentHero;

    public void Setup(HeroData hero)
    {
        currentHero = hero;

        bool unlocked =
            HeroProgressSystem.Instance.IsUnlocked(hero.heroType);

        // =========================
        // SOLO PRECIO REAL (SIN DESCUENTO)
        // =========================
        if (!unlocked)
        {
            int realPrice =
                HeroProgressSystem.Instance.GetGemUnlockPrice(hero.heroType);

            gemCostText.text = realPrice.ToString("N0");
            gemCostText.gameObject.SetActive(true);
        }
        else
        {
            gemCostText.gameObject.SetActive(false);
        }

        unlockButton.gameObject.SetActive(!unlocked);
        selectButton.gameObject.SetActive(unlocked);
        upgradeButton.gameObject.SetActive(unlocked);

        RefreshUpgrade();
    }

    void RefreshUpgrade()
    {
        if (currentHero == null)
            return;

        bool unlocked =
            HeroProgressSystem.Instance.IsUnlocked(currentHero.heroType);

        int level =
            HeroProgressSystem.Instance.GetLevel(currentHero.heroType);

        int fragCost = 10 * level;
        int goldCost = 100 * level;

        fragmentCostText.text = fragCost.ToString();
        goldCostText.text = goldCost.ToString();

        if (!unlocked)
        {
            upgradeButton.interactable = false;
            return;
        }

        if (HeroProgressSystem.Instance.IsMaxLevel(currentHero.heroType))
        {
            upgradeButton.interactable = false;
            return;
        }

        upgradeButton.interactable =
            HeroProgressSystem.Instance.CanUpgrade(currentHero.heroType);
    }

    public void OnUnlock()
    {
        if (HeroProgressSystem.Instance.TryUnlock(currentHero))
        {
            Setup(currentHero);
        }
    }

    public void OnUpgrade()
    {
        if (HeroProgressSystem.Instance.Upgrade(currentHero.heroType))
        {
            OnHeroUpgraded?.Invoke(currentHero);
            Setup(currentHero);
        }
    }

    public void OnSelect()
    {
        HeroSelectionManager.Instance.SelectHero(currentHero);
    }
}