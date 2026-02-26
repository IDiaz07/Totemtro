using UnityEngine;

public class RunLoadoutSystem : MonoBehaviour
{
    public static RunLoadoutSystem Instance;

    public InventorySlot[] loadoutSlots;
    public int maxSlots = 15;

    public System.Action onLoadoutChanged;

    public void NotifyLoadoutChanged()
    {
        onLoadoutChanged?.Invoke();
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        loadoutSlots = new InventorySlot[maxSlots];

        for (int i = 0; i < maxSlots; i++)
            loadoutSlots[i] = new InventorySlot(null, 0);
    }

    public void SetLoadoutFromMeta(MetaInventory metaInventory)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            loadoutSlots[i].Clear();

            if (i >= metaInventory.slots.Length)
                continue;

            if (!metaInventory.slots[i].IsEmpty())
            {
                loadoutSlots[i].item = metaInventory.slots[i].item;
                loadoutSlots[i].amount = metaInventory.slots[i].amount;
            }
        }
    }

    public int GetAmount(ItemData item)
    {
        int total = 0;

        foreach (var slot in loadoutSlots)
        {
            if (slot.item == item)
                total += slot.amount;
        }

        return total;
    }

    public void MoveItem(int from, int to)
    {
        if (from == to) return;

        var tempItem = loadoutSlots[to].item;
        var tempAmount = loadoutSlots[to].amount;

        loadoutSlots[to].item = loadoutSlots[from].item;
        loadoutSlots[to].amount = loadoutSlots[from].amount;

        loadoutSlots[from].item = tempItem;
        loadoutSlots[from].amount = tempAmount;

        NotifyLoadoutChanged();
    }

    public int FindSlotIndex(ItemData item)
    {
        for (int i = 0; i < loadoutSlots.Length; i++)
        {
            if (loadoutSlots[i].item == item)
                return i;
        }

        return -1;
    }
}