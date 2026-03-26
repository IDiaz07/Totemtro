using UnityEngine;
using System;

public class ChestInventory : MonoBehaviour
{
    public InventorySlot[] slots;

    public Action onChestChanged;

    public void Initialize(int size)
    {
        slots = new InventorySlot[size];

        for (int i = 0; i < size; i++)
            slots[i] = new InventorySlot(null, 0);
    }

    public void SetItem(int index, ItemData item, int amount)
    {
        slots[index].item = item;
        slots[index].amount = amount;

        onChestChanged?.Invoke();
    }

    public void ClearSlot(int index)
    {
        slots[index].Clear();
        onChestChanged?.Invoke();
    }
}