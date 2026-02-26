using UnityEngine;

[System.Serializable]
public class MetaSaveData
{
    public SlotSaveData[] slots;
    public int inventoryLevel;
}

public class MetaInventory : MonoBehaviour
{
    public static MetaInventory Instance;

    // =====================================
    // CONFIGURACIÓN DE NIVELES
    // =====================================

    public int baseSlots = 49;
    public int mediumSlots = 63;
    public int largeSlots = 77;

    public int inventoryLevel = 0;
    // 0 = Base (40)
    // 1 = Medium (55)
    // 2 = Large (65)

    public InventorySlot[] slots;

    public System.Action onInventoryChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadMetaInventory();
        InitializeSlots();
    }

    // =====================================
    // SLOT SIZE SEGÚN NIVEL
    // =====================================

    int GetSlotCount()
    {
        switch (inventoryLevel)
        {
            case 1: return mediumSlots;
            case 2: return largeSlots;
            default: return baseSlots;
        }
    }

    void InitializeSlots()
    {
        int slotCount = GetSlotCount();

        if (slots == null || slots.Length != slotCount)
        {
            InventorySlot[] newSlots = new InventorySlot[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                if (slots != null && i < slots.Length)
                    newSlots[i] = slots[i];
                else
                    newSlots[i] = new InventorySlot(null, 0);
            }

            slots = newSlots;
        }

        NotifyInventoryChanged();
    }

    // =====================================
    // UPGRADE INVENTORY
    // =====================================

    public void UpgradeInventory()
    {
        if (inventoryLevel >= 2)
            return;

        inventoryLevel++;
        InitializeSlots();
        SaveMetaInventory();
    }

    // =====================================
    // ADD ITEM
    // =====================================

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;

        // Merge stacks
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item &&
                slots[i].amount < item.maxStack)
            {
                int space = item.maxStack - slots[i].amount;
                int toAdd = Mathf.Min(space, remaining);

                slots[i].amount += toAdd;
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    SaveMetaInventory();
                    return true;
                }
            }
        }

        // Empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                int toAdd = Mathf.Min(item.maxStack, remaining);

                slots[i].item = item;
                slots[i].amount = toAdd;

                remaining -= toAdd;

                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    SaveMetaInventory();
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        SaveMetaInventory();
        return false;
    }

    // =====================================
    // REMOVE ITEM
    // =====================================

    public bool RemoveItem(ItemData item, int amount)
    {
        int remaining = amount;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                int remove = Mathf.Min(slots[i].amount, remaining);

                slots[i].amount -= remove;
                remaining -= remove;

                if (slots[i].amount <= 0)
                    slots[i].Clear();

                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    SaveMetaInventory();
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        SaveMetaInventory();
        return remaining <= 0;
    }

    // =====================================
    // MOVE ITEM
    // =====================================

    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= slots.Length) return;
        if (toIndex < 0 || toIndex >= slots.Length) return;

        InventorySlot temp = new InventorySlot(
            slots[toIndex].item,
            slots[toIndex].amount
        );

        slots[toIndex].item = slots[fromIndex].item;
        slots[toIndex].amount = slots[fromIndex].amount;

        slots[fromIndex].item = temp.item;
        slots[fromIndex].amount = temp.amount;

        NotifyInventoryChanged();
        SaveMetaInventory();
    }

    // =====================================
    // GET TOTAL AMOUNT
    // =====================================

    public int GetAmount(ItemData item)
    {
        int total = 0;

        foreach (var slot in slots)
        {
            if (slot.item == item)
                total += slot.amount;
        }

        return total;
    }

    // =====================================
    // SAVE / LOAD
    // =====================================

    public void SaveMetaInventory()
    {
        MetaSaveData data = new MetaSaveData();
        data.slots = new SlotSaveData[slots.Length];
        data.inventoryLevel = inventoryLevel;

        for (int i = 0; i < slots.Length; i++)
        {
            data.slots[i] = new SlotSaveData();

            if (!slots[i].IsEmpty())
            {
                data.slots[i].id = slots[i].item.itemID;
                data.slots[i].amount = slots[i].amount;
            }
            else
            {
                data.slots[i].id = "";
                data.slots[i].amount = 0;
            }
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("MetaInventory", json);
        PlayerPrefs.Save();
    }

    public void LoadMetaInventory()
    {
        if (!PlayerPrefs.HasKey("MetaInventory"))
        {
            inventoryLevel = 0;
            return;
        }

        string json = PlayerPrefs.GetString("MetaInventory");
        MetaSaveData data = JsonUtility.FromJson<MetaSaveData>(json);

        inventoryLevel = data.inventoryLevel;

        int slotCount = GetSlotCount();
        slots = new InventorySlot[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = new InventorySlot(null, 0);

            if (i >= data.slots.Length)
                continue;

            if (string.IsNullOrEmpty(data.slots[i].id))
                continue;

            ItemData item =
                ItemDatabase.Instance.GetItem(data.slots[i].id);

            if (item != null)
            {
                slots[i].item = item;
                slots[i].amount = data.slots[i].amount;
            }
        }
    }

    // =====================================
    // NOTIFY
    // =====================================

    public void NotifyInventoryChanged()
    {
        onInventoryChanged?.Invoke();
    }

    public int FindSlotIndex(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
                return i;
        }

        return -1;
    }
}