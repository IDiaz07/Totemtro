using UnityEngine;using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance;

    public ShopDatabase database;

    public static Action OnShopUpdated;

    Dictionary<string, ShopItemData> lookup = new();

    void Awake()
    {
        Instance = this;

        foreach (var item in database.allItems)
        {
            if (!lookup.ContainsKey(item.itemId))
                lookup.Add(item.itemId, item);
        }
    }

    // ==============================
    // GET ITEMS BY CATEGORY
    // ==============================

    public List<ShopItemData> GetItemsByCategory(ShopItemCategory category)
    {
        List<ShopItemData> result = new();

        foreach (var item in database.allItems)
        {
            if (item.category != category)
                continue;

            if (item.isFirstTimeOffer &&
                PlayerShopProfileSystem.Instance.HasMadeFirstPurchase())
                continue;

            result.Add(item);
        }

        return result;
    }

    // ==============================
    // PURCHASE
    // ==============================

    public bool TryPurchase(string id, Vector3 worldPos)
    {
        if (!lookup.ContainsKey(id))
            return false;

        var item = lookup[id];

        if (!ValidateCurrency(item))
            return false;

        float finalPrice = GetFinalPrice(item);

        ShopAnalytics.LogPurchase(item, (int)finalPrice);

        Spend(item);

        ShopPurchaseHandler.Execute(item, worldPos);

        OnShopUpdated?.Invoke();
        return true;
    }

    bool ValidateCurrency(ShopItemData item)
    {
        switch (item.priceCurrency)
        {
            case ShopCurrencyType.Gold:
                return MetaCurrencySystem.Instance.Gold
                    >= Mathf.RoundToInt(GetFinalPrice(item));

            case ShopCurrencyType.Gems:
                return MetaCurrencySystem.Instance.Gems
                    >= Mathf.RoundToInt(GetFinalPrice(item));

            case ShopCurrencyType.RealMoney:
                return true;
        }

        return false;
    }

    void Spend(ShopItemData item)
    {
        int finalPrice = Mathf.RoundToInt(GetFinalPrice(item));

        switch (item.priceCurrency)
        {
            case ShopCurrencyType.Gold:
                MetaCurrencySystem.Instance.SpendGold(finalPrice);
                break;

            case ShopCurrencyType.Gems:
                MetaCurrencySystem.Instance.SpendGems(finalPrice);
                break;
        }
    }

    // ==============================
    // PRICING
    // ==============================

    public float GetFinalPrice(ShopItemData item)
    {
        if (!item.hasDiscount)
            return item.priceAmount;

        float multiplier = 1f - (item.discountPercent / 100f);
        float final = item.priceAmount * multiplier;

        return Mathf.Max(0.01f, final);
    }

    public float GetDynamicPrice(ShopItemData item)
    {
        float basePrice = GetFinalPrice(item);

        if (PlayerShopProfileSystem.Instance.IsWhale())
            basePrice = basePrice * 1.1f;

        return basePrice;
    }

    public bool IsWhale()
    {
        return PlayerShopProfileSystem.Instance.IsWhale();
    }

    // ==============================
    // FEATURED
    // ==============================

    public List<ShopItemData> GetFeaturedItems()
    {
        var all = database.allItems;

        var legendary = all.FindAll(x => x.rarity == ShopRarity.Legendary);
        var epic = all.FindAll(x => x.rarity == ShopRarity.Epic);

        List<ShopItemData> featured = new();

        if (legendary.Count > 0)
            featured.Add(legendary[UnityEngine.Random.Range(0, legendary.Count)]);

        if (epic.Count > 0)
            featured.Add(epic[UnityEngine.Random.Range(0, epic.Count)]);

        return featured;
    }

    // ==============================
    // DAILY FRAGMENTS (FIXED)
    // ==============================

    public List<ShopItemData> GetDailyFragments()
    {
        var fragmentItems = database.allItems
            .FindAll(x => x.category == ShopItemCategory.Fragments);

        List<ShopItemData> result = new();
        HashSet<HeroData> usedHeroes = new();

        TryAddFragment(FragmentSize.Small);
        TryAddFragment(FragmentSize.Medium);
        TryAddFragment(FragmentSize.Big);

        return result;

        void TryAddFragment(FragmentSize size)
        {
            var candidates = fragmentItems
                .Where(x => x.fragmentSize == size &&
                            x.fragmentHero != null &&
                            !usedHeroes.Contains(x.fragmentHero))
                .ToList();

            if (candidates.Count == 0)
                return;

            var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            usedHeroes.Add(selected.fragmentHero);
            result.Add(selected);
        }
    }
}