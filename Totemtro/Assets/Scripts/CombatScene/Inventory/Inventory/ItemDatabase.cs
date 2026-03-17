using System.Collections.Generic;
    using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public ItemData[] allItems;

    Dictionary<string, ItemData> itemLookup =
        new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject); // 🔥 importante

        foreach (var item in allItems)
        {
            if (item == null)
            {
                Debug.LogWarning("ItemDatabase: null item in list");
                continue;
            }

            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogError(
                    "ItemDatabase: item without ID → " + item.name);
                continue;
            }

            if (itemLookup.ContainsKey(item.itemID))
            {
                Debug.LogWarning(
                    "Duplicate itemID: " + item.itemID);
                continue;
            }

            itemLookup.Add(item.itemID, item);
        }

        Debug.Log($"ItemDatabase loaded {itemLookup.Count} items");
    }

    public ItemData GetItem(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (itemLookup.ContainsKey(id))
            return itemLookup[id];

        return null;
    }
}

// Extensión segura que intenta resolver ItemData por id
public static class ItemDatabaseExtensions
{
    public static ItemData GetItemById(this ItemDatabase db, string id)
    {
        if (db == null || string.IsNullOrEmpty(id))
            return null;

        System.Type t = db.GetType();

        // 1) Si la clase ya tiene un método público que devuelve el item, invócalo (nombre comunes)
        string[] methodNames = { "GetItemById", "GetItem", "GetById", "FindById" };
        foreach (var name in methodNames)
        {
            System.Reflection.MethodInfo m = t.GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m != null)
            {
                try
                {
                    var res = m.Invoke(db, new object[] { id });
                    if (res is ItemData it) return it;
                }
                catch { /* ignorar y seguir intentando */ }
            }
        }

        // 2) Buscar colecciones/arrays comunes dentro de ItemDatabase y recorrer
        string[] fieldOrPropNames = { "items", "allItems", "itemList", "Items", "AllItems" };
        foreach (var name in fieldOrPropNames)
        {
            System.Reflection.FieldInfo f = t.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (f != null)
            {
                var val = f.GetValue(db) as System.Collections.IEnumerable;
                if (val != null)
                {
                    foreach (var o in val)
                    {
                        if (o is ItemData it && it.itemID == id) return it;
                    }
                }
            }

            System.Reflection.PropertyInfo p = t.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (p != null)
            {
                var val = p.GetValue(db) as System.Collections.IEnumerable;
                if (val != null)
                {
                    foreach (var o in val)
                    {
                        if (o is ItemData it && it.itemID == id) return it;
                    }
                }
            }
        }

        // 3) No encontrado
        return null;
    }
}