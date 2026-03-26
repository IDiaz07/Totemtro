using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MetaSaveData
{
    public SlotSaveData[] slots;      // inventory
    public SlotSaveData[] bagSlots;   // bag
    public SlotSaveData[] armorSlots; // armor
    public int inventoryLevel;
}

public class MetaInventory : MonoBehaviour
{
    public static MetaInventory Instance;

    public int baseSlots = 49;
    public int mediumSlots = 63;
    public int largeSlots = 77;

    // Nivel por defecto 1 → 49 slots
    public int inventoryLevel = 1;

    [HideInInspector]
    public InventorySlot[] slots;
    public InventorySlot[] bagSlots;
    public InventorySlot[] armorSlots;

    const int BAG_SIZE = 15;
    const int ARMOR_SIZE = 4;

    public System.Action onInventoryChanged;

    public bool IsInitialized { get; private set; } = false;
    public event Action OnInitialized;

    bool IsCombatScene()
    {
        return SceneManager.GetActiveScene().name.Contains("CombatScene");
    }

    void Awake()
    {
        Debug.Log("MetaInventory created: " + GetInstanceID());
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        StartCoroutine(Initialize());
    }

    void OnApplicationQuit()
    {
        SaveMetaInventory();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveMetaInventory();
    }

    IEnumerator Initialize()
    {
        if (SaveSystem.Instance == null)
        {
            var existing = FindObjectOfType<SaveSystem>();
            if (existing == null)
            {
                Debug.LogWarning("SaveSystem not found in scene — creating SaveSystem GameObject automatically.");
                var go = new GameObject("SaveSystem");
                go.AddComponent<SaveSystem>();
            }

            bool saveReady = false;
            System.Action onReady = () => saveReady = true;
            SaveSystem.OnReady += onReady;

            while (!saveReady)
                yield return null;

            SaveSystem.OnReady -= onReady;
        }
        else
        {
            while (!SaveSystem.Instance.IsReady)
                yield return null;
        }

        while (ItemDatabase.Instance == null)
            yield return null;

        Debug.Log("MetaInventory loading save...");

        LoadMetaInventory();

        int count = GetSlotCount();

        if (slots == null || slots.Length == 0)
        {
            slots = new InventorySlot[count];

            for (int i = 0; i < count; i++)
                slots[i] = new InventorySlot(null, 0);
        }

        InitializeSlots();

        if (bagSlots == null || bagSlots.Length != BAG_SIZE)
        {
            bagSlots = new InventorySlot[BAG_SIZE];

            for (int i = 0; i < BAG_SIZE; i++)
                bagSlots[i] = new InventorySlot(null, 0);
        }

        if (armorSlots == null || armorSlots.Length != ARMOR_SIZE)
        {
            armorSlots = new InventorySlot[ARMOR_SIZE];

            for (int i = 0; i < ARMOR_SIZE; i++)
                armorSlots[i] = new InventorySlot(null, 0);
        }

        Debug.Log("Inventory initialized with slots: " + slots.Length);

        IsInitialized = true;
        OnInitialized?.Invoke();

        NotifyInventoryChanged();
    }

    int GetSlotCount()
    {
        int count;

        switch (inventoryLevel)
        {
            case 1:
                count = baseSlots;
                break;

            case 2:
                count = mediumSlots;
                break;

            case 3:
                count = largeSlots;
                break;

            default:
                count = baseSlots;
                break;
        }

        if (count <= 0)
            count = baseSlots;

        return count;
    }

    void InitializeSlots()
    {
        int slotCount = Mathf.Max(GetSlotCount(), 1);

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
        if (inventoryLevel >= 3)
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
        if (!IsInitialized)
            return false;

        if (item == null || amount <= 0)
            return false;

        InventorySlot[] targetSlots =
            IsCombatScene() ? bagSlots : slots;

        int remaining = amount;

        // STACK
        for (int i = 0; i < targetSlots.Length; i++)
        {
            if (targetSlots[i].item == item &&
                targetSlots[i].amount < item.maxStack)
            {
                int space = item.maxStack - targetSlots[i].amount;
                int toAdd = Mathf.Min(space, remaining);

                targetSlots[i].amount += toAdd;
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    NotifyInventoryChanged();
                    SaveMetaInventory();
                    return true;
                }
            }
        }

        // EMPTY SLOT
        for (int i = 0; i < targetSlots.Length; i++)
        {
            if (targetSlots[i].IsEmpty())
            {
                int toAdd = Mathf.Min(item.maxStack, remaining);

                targetSlots[i].item = item;
                targetSlots[i].amount = toAdd;

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

        // ======================
        // HUB
        // ======================

        if (!IsCombatScene())
        {
            // INVENTORY
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
                        return true;
                }
            }

            // BAG
            for (int i = 0; i < bagSlots.Length; i++)
            {
                if (bagSlots[i].item == item)
                {
                    int remove = Mathf.Min(bagSlots[i].amount, remaining);

                    bagSlots[i].amount -= remove;
                    remaining -= remove;

                    if (bagSlots[i].amount <= 0)
                        bagSlots[i].Clear();

                    if (remaining <= 0)
                        return true;
                }
            }
        }

        // ======================
        // COMBAT
        // ======================

        else
        {
            // SOLO BAG (ActionBar usa bagSlots)
            for (int i = 0; i < bagSlots.Length; i++)
            {
                if (bagSlots[i].item == item)
                {
                    int remove = Mathf.Min(bagSlots[i].amount, remaining);

                    bagSlots[i].amount -= remove;
                    remaining -= remove;

                    if (bagSlots[i].amount <= 0)
                        bagSlots[i].Clear();

                    if (remaining <= 0)
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

        if (!IsCombatScene())
        {
            // BAG (ActionBar referencia bagSlots)
            foreach (var slot in bagSlots)
            {
                if (!slot.IsEmpty() && slot.item == item)
                    total += slot.amount;
            }
        }
        else
        {
            // HUB INVENTORY
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty() && slot.item == item)
                    total += slot.amount;
            }

            // HUB BAG
            foreach (var slot in bagSlots)
            {
                if (!slot.IsEmpty() && slot.item == item)
                    total += slot.amount;
            }
        }

        return total;
    }

    // =====================================
    // SAVE / LOAD
    // =====================================
    public void SaveMetaInventory()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("SaveSystem not ready");
            return;
        }

        MetaSaveData data = new MetaSaveData();

        data.inventoryLevel = inventoryLevel;

        // =====================================
        // INVENTORY
        // =====================================
        data.slots = new SlotSaveData[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            data.slots[i] = new SlotSaveData();

            if (!slots[i].IsEmpty())
            {
                data.slots[i].id = slots[i].item.itemID;
                data.slots[i].amount = slots[i].amount;
            }
        }

        // =====================================
        // BAG
        // =====================================
        data.bagSlots = new SlotSaveData[bagSlots != null ? bagSlots.Length : 0];

        for (int i = 0; i < data.bagSlots.Length; i++)
        {
            data.bagSlots[i] = new SlotSaveData();

            if (!bagSlots[i].IsEmpty())
            {
                data.bagSlots[i].id = bagSlots[i].item.itemID;
                data.bagSlots[i].amount = bagSlots[i].amount;
            }
        }

        // =====================================
        // ARMOR
        // =====================================
        // 🔥 SINCRONIZAR EQUIPMENT → ARMOR SLOTS
        if (EquipmentSystem.Instance != null)
        {
            var eq = EquipmentSystem.Instance.equipmentSlots;

            for (int i = 0; i < eq.Length; i++)
            {
                if (armorSlots[i] == null)
                    armorSlots[i] = new InventorySlot(null, 0);

                armorSlots[i].item = eq[i].item;
                armorSlots[i].amount = eq[i].amount;
            }
        }

        string json = JsonUtility.ToJson(data);

        SaveSystem.Instance.Save("MetaInventory", json);

        Debug.Log("Inventory Saved: " + json);
    }

    public void LoadMetaInventory()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogError("SaveSystem not ready");
            return;
        }

        int totalSlots = GetSlotCount();

        string json = SaveSystem.Instance.Load("MetaInventory");

        Debug.Log("Loaded inventory json: " + json);

        // =====================================
        // NO SAVE
        // =====================================
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("No inventory save found");

            inventoryLevel = 1;

            totalSlots = GetSlotCount();

            slots = new InventorySlot[totalSlots];

            for (int i = 0; i < totalSlots; i++)
                slots[i] = new InventorySlot(null, 0);

            bagSlots = new InventorySlot[BAG_SIZE];

            for (int i = 0; i < BAG_SIZE; i++)
                bagSlots[i] = new InventorySlot(null, 0);

            armorSlots = new InventorySlot[ARMOR_SIZE];

            for (int i = 0; i < ARMOR_SIZE; i++)
                armorSlots[i] = new InventorySlot(null, 0);

            return;
        }

        MetaSaveData data = JsonUtility.FromJson<MetaSaveData>(json);

        if (data == null || data.slots == null)
        {
            Debug.LogWarning("Save corrupted. Creating new inventory.");

            inventoryLevel = 1;

            totalSlots = GetSlotCount();

            slots = new InventorySlot[totalSlots];

            for (int i = 0; i < totalSlots; i++)
                slots[i] = new InventorySlot(null, 0);

            bagSlots = new InventorySlot[BAG_SIZE];

            for (int i = 0; i < BAG_SIZE; i++)
                bagSlots[i] = new InventorySlot(null, 0);

            armorSlots = new InventorySlot[ARMOR_SIZE];

            for (int i = 0; i < ARMOR_SIZE; i++)
                armorSlots[i] = new InventorySlot(null, 0);

            return;
        }

        inventoryLevel = Mathf.Clamp(data.inventoryLevel, 1, 3);

        totalSlots = GetSlotCount();

        // =====================================
        // INVENTORY
        // =====================================
        slots = new InventorySlot[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            slots[i] = new InventorySlot(null, 0);

            if (i >= data.slots.Length)
                continue;

            if (string.IsNullOrEmpty(data.slots[i].id))
                continue;

            ItemData item = ItemDatabase.Instance.GetItemById(data.slots[i].id);

            if (item != null)
            {
                slots[i].item = item;
                slots[i].amount = data.slots[i].amount;
            }
        }

        // =====================================
        // BAG
        // =====================================
        bagSlots = new InventorySlot[BAG_SIZE];

        for (int i = 0; i < BAG_SIZE; i++)
        {
            bagSlots[i] = new InventorySlot(null, 0);

            if (data.bagSlots == null || i >= data.bagSlots.Length)
                continue;

            if (string.IsNullOrEmpty(data.bagSlots[i].id))
                continue;

            ItemData item = ItemDatabase.Instance.GetItemById(data.bagSlots[i].id);

            if (item != null)
            {
                bagSlots[i].item = item;
                bagSlots[i].amount = data.bagSlots[i].amount;
            }
        }

        // =====================================
        // ARMOR
        // =====================================
        // 🔥 APLICAR ARMOR A EQUIPMENT SYSTEM
        if (EquipmentSystem.Instance != null)
        {
            for (int i = 0; i < armorSlots.Length; i++)
            {
                var slot = armorSlots[i];

                if (slot != null && slot.item != null)
                {
                    EquipmentSystem.Instance.equipmentSlots[i].item = slot.item;
                    EquipmentSystem.Instance.equipmentSlots[i].amount = slot.amount;
                }
            }

            EquipmentSystem.Instance.onEquipmentChanged?.Invoke();
        }

        Debug.Log("MetaInventory loaded successfully");
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

    public void MarkInitialized()
    {
        IsInitialized = true;
    }
}