using UnityEngine;

public class ActionBarController : MonoBehaviour
{
    public RunInventory inventory;
    public ActionSlot[] slots = new ActionSlot[8];

    HeroController hero;

    void Start()
    {
        hero = FindFirstObjectByType<HeroController>();
    }

    KeyCode[] keys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8
    };

    void Awake()
    {
        if (slots == null || slots.Length != 8)
            slots = new ActionSlot[8];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new ActionSlot();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateCooldowns();
    }

    void HandleInput()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
                TryUseSlot(i);
        }
    }

    void UpdateCooldowns()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].cooldownRemaining > 0f)
            {
                slots[i].cooldownRemaining -= Time.deltaTime;

                if (slots[i].cooldownRemaining < 0f)
                    slots[i].cooldownRemaining = 0f;
            }
        }
    }

    public void AssignToSlot(int inventoryIndex, int actionIndex)
    {
        if (inventoryIndex < 0 || inventoryIndex >= inventory.slots.Length)
            return;

        if (actionIndex < 0 || actionIndex >= slots.Length)
            return;

        var invSlot = inventory.slots[inventoryIndex];

        if (invSlot.IsEmpty())
            return;

        // 🔥 SOLO CONSUMIBLES
        if (invSlot.item.itemType != ItemType.Consumable)
            return;

        slots[actionIndex].item = invSlot.item;
        slots[actionIndex].amount = invSlot.amount;

        inventory.slots[inventoryIndex].Clear();
        inventory.NotifyInventoryChanged();
    }

    void TryUseSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        var slot = slots[index];

        if (slot.IsEmpty())
            return;

        if (slot.cooldownRemaining > 0f)
            return;

        if (slot.item.ability == null)
            return;

        bool activated = slot.item.ability.TryActivate(hero.gameObject);

        if (!activated)
            return;

        slot.amount--;

        if (slot.amount <= 0)
        {
            slot.Clear();
            return;
        }

        slot.cooldownRemaining = slot.item.cooldown;
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        slots[index].Clear();
    }

    public int AddToExistingStacks(ItemData item, int amount)
    {
        int remaining = amount;

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (slot.IsEmpty())
                continue;

            if (slot.item != item)
                continue;

            int space = item.maxStack - slot.amount;

            if (space <= 0)
                continue;

            int toAdd = Mathf.Min(space, remaining);

            slot.amount += toAdd;
            remaining -= toAdd;

            if (remaining <= 0)
                break;
        }

        return remaining; // lo que NO se pudo meter
    }

    public int AddConsumable(ItemData item, int amount)
    {
        if (item.itemType != ItemType.Consumable)
            return amount;

        int remaining = amount;

        // 1️⃣ Stackear en slots existentes
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (slot.IsEmpty())
                continue;

            if (slot.item != item)
                continue;

            int space = item.maxStack - slot.amount;

            if (space <= 0)
                continue;

            int toAdd = Mathf.Min(space, remaining);

            slot.amount += toAdd;
            remaining -= toAdd;

            if (remaining <= 0)
                return 0;
        }

        // 2️⃣ Crear nuevo stack en slot vacío
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (!slot.IsEmpty())
                continue;

            int toAdd = Mathf.Min(item.maxStack, remaining);

            slot.item = item;
            slot.amount = toAdd;

            remaining -= toAdd;

            if (remaining <= 0)
                return 0;
        }

        return remaining;
    }

    public bool HasItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty() &&
                slots[i].item == item)
                return true;
        }

        return false;
    }
}