using UnityEngine;

public class ActionBarController : MonoBehaviour
{
    public MetaInventory inventory;
    public ActionSlot[] slots = new ActionSlot[8];

    public static ActionBarController Instance;

    HeroController hero;

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
        Instance = this;

        if (inventory == null)
            inventory = MetaInventory.Instance;

        if (slots == null || slots.Length != 8)
            slots = new ActionSlot[8];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new ActionSlot();
        }
    }

    void Start()
    {
        hero = FindFirstObjectByType<HeroController>();
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

    // ============================
    // USE ITEM
    // ============================

    void TryUseSlot(int index)
    {
        var slot = slots[index];

        if (slot.IsEmpty())
            return;

        if (slot.item.ability == null)
            return;

        if (slot.cooldownRemaining > 0f)
            return;

        bool activated = slot.item.ability.TryActivate(hero.gameObject);

        if (!activated)
            return;

        slot.cooldownRemaining = slot.item.cooldown;

        slot.amount--;

        if (slot.amount <= 0)
            slot.Clear();

        inventory.NotifyInventoryChanged();
    }

    // ============================
    // CLEAR SLOT
    // ============================

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        slots[index].Clear();
    }

    // ============================
    // ADD ITEM FROM BAG
    // ============================

    public void AssignToSlot(int bagIndex, int actionIndex)
    {
        if (inventory == null)
            return;

        if (bagIndex < 0 || bagIndex >= inventory.bagSlots.Length)
            return;

        if (actionIndex < 0 || actionIndex >= slots.Length)
            return;

        var bagSlot = inventory.bagSlots[bagIndex];

        if (bagSlot.IsEmpty())
            return;

        if (bagSlot.item.itemType != ItemType.Consumable)
            return;

        var actionSlot = slots[actionIndex];

        actionSlot.item = bagSlot.item;
        actionSlot.amount = bagSlot.amount;

        bagSlot.Clear();

        inventory.NotifyInventoryChanged();
    }

    public bool TryAddItem(ItemData item, int amount)
    {
        if (item.itemType != ItemType.Consumable)
            return false;

        // 1️⃣ STACK EN SLOT EXISTENTE
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

            int toAdd = Mathf.Min(space, amount);

            slot.amount += toAdd;
            amount -= toAdd;

            if (amount <= 0)
                return true;
        }

        // 2️⃣ SLOT VACÍO
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (!slot.IsEmpty())
                continue;

            int toAdd = Mathf.Min(item.maxStack, amount);

            slot.item = item;
            slot.amount = toAdd;

            amount -= toAdd;

            if (amount <= 0)
                return true;
        }

        // 3️⃣ NO HAY ESPACIO
        return false;
    }
    // ============================
    // CHECK ITEM
    // ============================

    public bool HasItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (!slot.IsEmpty() &&
                slot.item == item)
                return true;
        }

        return false;
    }

    // ============================
    // CONSUME ITEMS
    // ============================

    public void ConsumeBandage()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (slot.IsEmpty())
                continue;

            if (slot.item.ability is ThrallsBandageAbility)
            {
                slot.amount--;

                if (slot.amount <= 0)
                    slot.Clear();

                inventory.NotifyInventoryChanged();
                return;
            }
        }
    }

    public void ConsumePotion()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (slot.IsEmpty())
                continue;

            if (slot.item.ability is SmallHealthPotionAbility)
            {
                slot.amount--;

                if (slot.amount <= 0)
                    slot.Clear();

                inventory.NotifyInventoryChanged();
                return;
            }
        }
    }
}