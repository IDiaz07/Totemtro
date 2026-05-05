using UnityEngine;
using System.Collections.Generic;

public enum FragmentSize
{
    None,
    Small,
    Medium,
    Big
}

[CreateAssetMenu(fileName = "ShopItem", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("ID")]
    public string itemId;

    [Header("Category")]
    public ShopItemCategory category;

    [Header("Visual")]
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public ShopRarity rarity;

    [Header("Tags")]
    public bool isHot;

    [Header("Limited Offer")]
    public bool isLimited;
    public int durationHours;

    [Header("Type")]
    public ShopItemType itemType;

    [Header("Pricing")]
    public ShopCurrencyType priceCurrency;
    public int priceAmount;

    [Header("Currency Reward")]
    public int goldAmount;
    public int gemAmount;

    [Header("Hero Reward")]
    public HeroData heroReward;

    [Header("Fragment")]
    public HeroData fragmentHero;
    public FragmentSize fragmentSize;
    public int fragmentAmount;
    public Sprite fragmentVisualIcon;

    [Header("Bundle Rewards")]
    public List<ShopBundleReward> bundleRewards;

    [Header("Discount")]
    public bool hasDiscount;
    [Range(0, 100)] public int discountPercent;

    [Header("Special Flags")]
    public bool isFirstTimeOffer;

    [Header("Progression Bundle")]
    public string progressionGroupId;
    public int progressionStep;
}