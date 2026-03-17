using UnityEngine;

[System.Serializable]
public class InventoryContainer
{
    public InventorySlot[] slots;

    public int Size => slots != null ? slots.Length : 0;

    public InventoryContainer(int size)
    {
        slots = new InventorySlot[size];

        for (int i = 0; i < size; i++)
            slots[i] = new InventorySlot(null, 0);
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }
}