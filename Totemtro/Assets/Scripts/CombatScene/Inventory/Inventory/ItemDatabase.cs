using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public ItemData[] allItems;

    Dictionary<string, ItemData> itemLookup =
        new Dictionary<string, ItemData>();

    void Awake()
    {
        Instance = this;

        foreach (var item in allItems)
        {
            if (!itemLookup.ContainsKey(item.itemID))
                itemLookup.Add(item.itemID, item);
        }
    }

    public ItemData GetItem(string id)
    {
        if (itemLookup.ContainsKey(id))
            return itemLookup[id];

        return null;
    }
}