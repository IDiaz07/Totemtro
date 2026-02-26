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

    HeroData currentHero;

    public void Setup(HeroData hero)
    {
        currentHero = hero;

        bool unlocked =
            HeroProgressSystem.Instance.IsUnlocked(hero.heroType);

        unlockButton.gameObject.SetActive(!unlocked);
        selectButton.gameObject.SetActive(unlocked);

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

        // 🔥 NUEVA LÓGICA PROFESIONAL
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