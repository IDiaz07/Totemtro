using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Shop/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public List<ShopItemData> allItems;
}