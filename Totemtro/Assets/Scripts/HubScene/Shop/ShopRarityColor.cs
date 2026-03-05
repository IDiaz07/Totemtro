using UnityEngine;

public static class ShopRarityColor
{
    public static Color GetColor(ShopRarity rarity)
    {
        switch (rarity)
        {
            case ShopRarity.Common: return Color.gray;
            case ShopRarity.Rare: return Color.blue;
            case ShopRarity.Epic: return new Color(0.6f, 0f, 1f);
            case ShopRarity.Legendary: return new Color(1f, 0.5f, 0f);
        }
        return Color.white;
    }
}