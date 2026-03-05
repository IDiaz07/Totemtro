using UnityEngine;

public static class ShopPurchaseHandler
{
    public static void Execute(ShopItemData item, Vector3 worldPos)
    {
        switch (item.itemType)
        {
            case ShopItemType.Currency:

                if (item.goldAmount > 0)
                    MetaCurrencySystem.Instance.AddGold(item.goldAmount, worldPos);

                if (item.gemAmount > 0)
                    MetaCurrencySystem.Instance.AddGems(item.gemAmount, worldPos);

                break;

            case ShopItemType.Hero:

                if (item.heroReward != null)
                    HeroProgressSystem.Instance.TryUnlock(item.heroReward);

                break;

            case ShopItemType.HeroFragments:

                FragmentFlyAnimationSystem.Instance.PlayFragmentFly(
                    item.icon,
                    worldPos,
                    item.fragmentHero.heroType,
                    item.fragmentAmount
                );

                break;

            case ShopItemType.Bundle:

                HandleBundle(item, worldPos);

                break;
        }

        // ==========================
        // REGISTER PROFILE
        // ==========================

        if (PlayerShopProfileSystem.Instance != null)
        {
            PlayerShopProfileSystem.Instance.RegisterPurchase(
                item.priceCurrency == ShopCurrencyType.Gold ? item.priceAmount : 0,
                item.priceCurrency == ShopCurrencyType.Gems ? item.priceAmount : 0
            );
        }

        // ==========================
        // GACHA
        // ==========================

        if (GachaRevealPanel.Instance != null)
            GachaRevealPanel.Instance.Show(item);

        // ==========================
        // BUNDLE PROGRESSION
        // ==========================

        if (BundleProgressionSystem.Instance != null)
            BundleProgressionSystem.Instance.RegisterBundlePurchase(item);
    }

    static void HandleBundle(ShopItemData item, Vector3 worldPos)
    {
        if (item.bundleRewards == null)
            return;

        foreach (var reward in item.bundleRewards)
        {
            switch (reward.rewardType)
            {
                case ShopItemType.Currency:

                    if (reward.amount > 0)
                        MetaCurrencySystem.Instance.AddGold(reward.amount, worldPos);

                    break;

                case ShopItemType.Hero:

                    if (reward.hero != null)
                        HeroProgressSystem.Instance.TryUnlock(reward.hero);

                    break;

                case ShopItemType.HeroFragments:

                    if (reward.hero != null)
                        HeroProgressSystem.Instance.AddFragments(
                            reward.hero.heroType,
                            reward.amount
                        );

                    break;
            }
        }
    }
}