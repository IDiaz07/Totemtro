using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int amount;

    public InventorySlot(ItemData item, int amount)
    {
        this.item = item;
        this.amount = amount;
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

public class RunInventory : MonoBehaviour
{
    public int maxSlots = 15;
    public InventorySlot[] slots;

    public System.Action onInventoryChanged;

    void Awake()
    {
        slots = new InventorySlot[maxSlots];

        for (int i = 0; i < maxSlots; i++)
            slots[i] = new InventorySlot(null, 0);
    }

    public void NotifyInventoryChanged()
    {
        onInventoryChanged?.Invoke();
    }

    // =============================
    // ADD
    // =============================

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
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        return false;
    }

    // =============================
    // REMOVE
    // =============================

    public void RemoveItem(int index, int amount)
    {
        if (index < 0 || index >= slots.Length)
            return;

        if (slots[index].IsEmpty())
            return;

        slots[index].amount -= amount;

        if (slots[index].amount <= 0)
        {
            slots[index].Clear();
        }

        NotifyInventoryChanged();
    }

    // =============================
    // MOVE
    // =============================

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
    }

    // =============================
    // SAVE
    // =============================

    public PlayerSaveData CreateSaveData(ActionBarController actionBar)
    {
        PlayerSaveData data = new PlayerSaveData();

        // INVENTORY
        data.inventory = new SlotSaveData[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            data.inventory[i] = new SlotSaveData();

            if (!slots[i].IsEmpty())
            {
                data.inventory[i].id = slots[i].item.itemID;
                data.inventory[i].amount = slots[i].amount;
            }
            else
            {
                data.inventory[i].id = "";
                data.inventory[i].amount = 0;
            }
        }

        return data;
    }

    public void LoadFromSave(PlayerSaveData data,
                             ActionBarController actionBar)
    {
        // INVENTORY
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Clear();

            if (string.IsNullOrEmpty(data.inventory[i].id))
                continue;

            ItemData item =
                ItemDatabase.Instance.GetItem(data.inventory[i].id);

            if (item != null)
            {
                slots[i].item = item;
                slots[i].amount = data.inventory[i].amount;
            }
        }

        NotifyInventoryChanged();
    }

    // =============================
    // UTIL
    // =============================

    public bool CanAddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item &&
                slots[i].amount < item.maxStack)
            {
                remaining -= (item.maxStack - slots[i].amount);
                if (remaining <= 0)
                    return true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                remaining -= item.maxStack;
                if (remaining <= 0)
                    return true;
            }
        }

        return false;
    }

    public int GetAmount(ItemData item)
    {
        int total = 0;

        foreach (var s in slots)
            if (s.item == item)
                total += s.amount;

        return total;
    }

    public void DropOne(int index)
    {
        DropMultiple(index, 1);
    }

    public void DropHalf(int index)
    {
        if (slots[index].IsEmpty()) return;

        int amount = slots[index].amount / 2;
        if (amount <= 0) return;

        DropMultiple(index, amount);
    }

    public void DropStack(int index)
    {
        if (slots[index].IsEmpty()) return;

        int amount = slots[index].amount;
        DropMultiple(index, amount);
    }

    void DropMultiple(int index, int amount)
    {
        if (index < 0 || index >= slots.Length)
            return;

        if (slots[index].IsEmpty())
            return;

        ItemData item = slots[index].item;

        for (int i = 0; i < amount; i++)
            SpawnDrop(item);

        RemoveItem(index, amount);
    }

    void SpawnDrop(ItemData item)
    {
        if (item == null || item.worldPrefab == null)
            return;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector2 dir =
            (mouseWorld - player.transform.position).normalized;

        Vector3 spawnPos =
            player.transform.position + (Vector3)(dir * 1.2f);

        GameObject drop =
            Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);

        MaterialDrop mat =
            drop.GetComponent<MaterialDrop>();

        if (mat != null)
            mat.Initialize(item, 1, true);

        Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dir * 5f, ForceMode2D.Impulse);
        }

        // Sonido
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
            audio.Play();

        // Recoil jugador
        PlayerMovement movement =
            player.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.ApplyRecoil(dir, 1.2f);
    }
}