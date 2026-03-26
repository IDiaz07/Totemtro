using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int amount;

    public int durability;

    public InventorySlot(ItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;

        if (item != null)
            durability = item.maxDurability;
    }

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }
}