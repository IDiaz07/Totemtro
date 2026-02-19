using UnityEngine;
using System.Collections.Generic;

public class MaterialInventory : MonoBehaviour
{
    Dictionary<ItemData, int> materials = new Dictionary<ItemData, int>();

    public void Add(ItemData item, int amount = 1)
    {
        if (!materials.ContainsKey(item))
            materials[item] = 0;

        materials[item] += amount;
    }

    public bool Has(ItemData item, int amount)
    {
        return materials.ContainsKey(item) && materials[item] >= amount;
    }

    public void Remove(ItemData item, int amount)
    {
        if (!materials.ContainsKey(item)) return;

        materials[item] -= amount;

        if (materials[item] <= 0)
            materials.Remove(item);
    }

    public Dictionary<ItemData, int> GetAll()
    {
        return materials;
    }
}
