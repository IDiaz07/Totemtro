using UnityEngine;

public static class ShopAnalytics
{
    public static void LogPurchase(ShopItemData item, int finalPrice)
    {
        Debug.Log($"Purchased: {item.itemId} | Price: {finalPrice}");

        // Aquí iría:
        // FirebaseAnalytics.LogEvent(...)
        // GameAnalytics.NewBusinessEvent(...)
    }

    public static void LogShopOpen()
    {
        Debug.Log("Shop Opened");
    }
}