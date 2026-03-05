using UnityEngine;

[System.Serializable]
public class ShopBundleReward
{
    public ShopItemType rewardType;

    [Header("Currency")]
    public ShopCurrencyType currencyType;

    [Header("Amount")]
    public int amount;

    [Header("Hero")]
    public HeroData hero;
}