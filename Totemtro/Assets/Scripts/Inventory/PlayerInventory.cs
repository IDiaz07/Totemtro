using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<InventorySlot> items = new List<InventorySlot>();
    public System.Action onInventoryChanged;

    [Header("Inventory Settings")]
    public int maxSlots = 15;

    public void NotifyInventoryChanged()
    {
        onInventoryChanged?.Invoke();
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;

        // 1️⃣ Rellenar stacks existentes
        foreach (var slot in items)
        {
            if (slot.item == item && slot.amount < item.maxStack)
            {
                int space = item.maxStack - slot.amount;
                int toAdd = Mathf.Min(space, remaining);

                slot.amount += toAdd;
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        // 2️⃣ Crear nuevos stacks si hay espacio
        while (remaining > 0)
        {
            if (items.Count >= maxSlots)
            {
                NotifyInventoryChanged();
                return false; // ❌ inventario lleno
            }

            int toAdd = Mathf.Min(item.maxStack, remaining);
            items.Add(new InventorySlot(item, toAdd));
            remaining -= toAdd;
        }

        SortAndMerge();
        NotifyInventoryChanged();
        return true;
    }

    public bool IsFull()
    {
        return items.Count >= maxSlots;
    }

    public bool HasItem(ItemData item, int amount)
    {
        InventorySlot slot = items.Find(i => i.item == item);
        return slot != null && slot.amount >= amount;
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].item == item)
            {
                if (items[i].amount <= remaining)
                {
                    remaining -= items[i].amount;
                    items.RemoveAt(i);
                }
                else
                {
                    items[i].amount -= remaining;
                    remaining = 0;
                }

                if (remaining <= 0)
                {
                    SortAndMerge();
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        return false;
    }

    void SortAndMerge()
    {
        Dictionary<ItemData, int> totalAmounts =
            new Dictionary<ItemData, int>();

        // 1️⃣ Sumar cantidades totales por item
        foreach (var slot in items)
        {
            if (totalAmounts.ContainsKey(slot.item))
                totalAmounts[slot.item] += slot.amount;
            else
                totalAmounts.Add(slot.item, slot.amount);
        }

        items.Clear();

        // 2️⃣ Recrear stacks respetando maxStack
        foreach (var pair in totalAmounts)
        {
            ItemData item = pair.Key;
            int remaining = pair.Value;

            while (remaining > 0)
            {
                int toAdd = Mathf.Min(item.maxStack, remaining);
                items.Add(new InventorySlot(item, toAdd));
                remaining -= toAdd;
            }
        }

        // 3️⃣ Orden opcional (por nombre aquí)
        items.Sort((a, b) =>
            string.Compare(a.item.itemName, b.item.itemName));
    }

    public int GetAmount(ItemData item)
    {
        InventorySlot slot = items.Find(i => i.item == item);
        return slot != null ? slot.amount : 0;
    }
}
