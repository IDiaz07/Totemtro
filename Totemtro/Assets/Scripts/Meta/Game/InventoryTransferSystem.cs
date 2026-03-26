using UnityEngine;

public static class InventoryTransferSystem
{
    static InventorySlot[] GetSlotsFor(DragSource src)
    {
        switch (src)
        {
            case DragSource.Meta:
                return MetaInventory.Instance?.slots;

            case DragSource.Bag:
                return MetaInventory.Instance?.bagSlots;

            case DragSource.Armor:
                return EquipmentSystem.Instance?.equipmentSlots;

            case DragSource.Chest:
                return ChestUI.CurrentChest?.slots;

            default:
                return null;
        }
    }

    // ===================================
    // TRANSFER STACK (GENERIC)
    // ===================================

    public static bool TransferStack(
        InventorySlot[] source,
        int fromIndex,
        InventorySlot[] target)
    {
        var meta = MetaInventory.Instance;

        if (source == null || target == null)
            return false;

        if (fromIndex < 0 || fromIndex >= source.Length)
            return false;

        var src = source[fromIndex];

        if (src.IsEmpty())
            return false;

        ItemData item = src.item;
        int remaining = src.amount;

        // STACK FIRST
        foreach (var slot in target)
        {
            if (slot.item != item || slot.durability != src.durability)
                continue;

            int space = item.maxStack - slot.amount;
            int add = Mathf.Min(space, remaining);

            slot.amount += add;
            remaining -= add;

            if (remaining <= 0)
                break;
        }

        // EMPTY SLOT
        foreach (var slot in target)
        {
            if (remaining <= 0)
                break;

            if (slot.IsEmpty())
            {
                slot.item = item;
                slot.amount = remaining;
                slot.durability = src.durability;
                remaining = 0;
            }
        }

        src.amount = remaining;

        if (src.amount <= 0)
            src.Clear();

        meta.NotifyInventoryChanged();
        meta.SaveMetaInventory();

        return true;
    }

    // ===================================
    // MOVE AMOUNT (DRAG)
    // ===================================

    public static bool MoveAmount(
        DragSource source,
        int fromIndex,
        DragSource target,
        int toIndex,
        int amount)
    {
        var meta = MetaInventory.Instance;

        if (meta == null)
            return false;

        // ❌ BLOQUEAR uso normal en armor
        if (target == DragSource.Armor || source == DragSource.Armor)
        {
            Debug.Log("Use Equip system instead of TransferSystem for armor");
            return false;
        }

        var srcSlots = GetSlotsFor(source);
        var dstSlots = GetSlotsFor(target);

        if (srcSlots == null || dstSlots == null)
            return false;

        if (fromIndex < 0 || fromIndex >= srcSlots.Length)
            return false;

        if (toIndex < 0 || toIndex >= dstSlots.Length)
            return false;

        var src = srcSlots[fromIndex];
        var dst = dstSlots[toIndex];

        if (src.IsEmpty())
            return false;

        int moveAmount = Mathf.Min(amount, src.amount);

        // 🔥 STACK (solo si misma durability)
        if (dst.item != null && dst.item == src.item && dst.durability == src.durability)
        {
            int space = dst.item.maxStack - dst.amount;
            int toAdd = Mathf.Min(space, moveAmount);

            dst.amount += toAdd;
            src.amount -= toAdd;

            if (src.amount <= 0)
                src.Clear();
        }
        else if (dst.IsEmpty())
        {
            dst.item = src.item;
            dst.amount = moveAmount;
            dst.durability = src.durability; // 🔥 FIX

            src.amount -= moveAmount;

            if (src.amount <= 0)
                src.Clear();
        }
        else
        {
            // 🔥 SWAP COMPLETO
            var tempItem = dst.item;
            var tempAmount = dst.amount;
            var tempDurability = dst.durability;

            dst.item = src.item;
            dst.amount = src.amount;
            dst.durability = src.durability;

            src.item = tempItem;
            src.amount = tempAmount;
            src.durability = tempDurability;
        }

        meta.NotifyInventoryChanged();
        meta.SaveMetaInventory();

        return true;
    }

    // ===================================
    // MOVE FULL STACK (SHIFT CLICK)
    // ===================================

    public static bool MoveFullStack(
        DragSource source,
        int fromIndex,
        DragSource target)
    {
        var meta = MetaInventory.Instance;
        var actionBar = ActionBarController.Instance;

        if (meta == null)
            return false;

        // =========================
        // BAG → ACTIONBAR
        // =========================

        if (source == DragSource.Bag && target == DragSource.ActionBar)
        {
            var bagSlot = meta.bagSlots[fromIndex];

            if (bagSlot.IsEmpty())
                return false;

            if (bagSlot.item.itemType != ItemType.Consumable)
                return false;

            bool added = actionBar.TryAddItem(
                bagSlot.item,
                bagSlot.amount
            );

            if (!added)
            {
                var shake = Object.FindFirstObjectByType<UIShake>();

                if (shake != null)
                    shake.Play();

                return false;
            }

            bagSlot.Clear();

            meta.NotifyInventoryChanged();
            return true;
        }

        // =========================
        // ACTION BAR → BAG
        // =========================
        if (source == DragSource.ActionBar && target == DragSource.Bag)
        {
            var slot = actionBar.slots[fromIndex];

            if (slot == null || slot.item == null)
                return false;

            var bag = meta.bagSlots;

            ItemData item = slot.item;
            int amount = slot.amount;

            // 🔹 STACK FIRST
            foreach (var b in bag)
            {
                if (b.item != item)
                    continue;

                int space = item.maxStack - b.amount;
                int add = Mathf.Min(space, amount);

                b.amount += add;
                amount -= add;

                if (amount <= 0)
                    break;
            }

            // 🔹 EMPTY SLOT
            foreach (var b in bag)
            {
                if (amount <= 0)
                    break;

                if (b.IsEmpty())
                {
                    b.item = item;
                    b.amount = amount;
                    amount = 0;
                }
            }

            // 🔹 UPDATE ACTION BAR
            slot.amount = amount;

            if (slot.amount <= 0)
            {
                slot.item = null;
                slot.amount = 0;
            }

            meta.NotifyInventoryChanged();
            return true;
        }

        // =========================
        // INVENTORY → BAG
        // =========================

        if (source == DragSource.Meta && target == DragSource.Bag)
        {
            return TransferStack(
                meta.slots,
                fromIndex,
                meta.bagSlots
            );
        }

        // =========================
        // BAG → INVENTORY
        // =========================

        if (source == DragSource.Bag && target == DragSource.Meta)
        {
            return TransferStack(
                meta.bagSlots,
                fromIndex,
                meta.slots
            );
        }
        // =========================
        // CHEST → BAG
        // =========================
        if (source == DragSource.Chest && target == DragSource.Bag)
        {
            return TransferStack(
                ChestUI.CurrentChest?.slots,
                fromIndex,
                meta.bagSlots
            );
        }

        // =========================
        // BAG → CHEST
        // =========================
        if (source == DragSource.Bag && target == DragSource.Chest)
        {
            return TransferStack(
                meta.bagSlots,
                fromIndex,
                ChestUI.CurrentChest?.slots
            );
        }

        // =========================
        // ARMOR → META (INVENTORY)
        // =========================
        if (source == DragSource.Armor && target == DragSource.Meta)
        {
            var eqSlots = EquipmentSystem.Instance?.equipmentSlots;
            var metaSlots = meta.slots;

            if (eqSlots == null || metaSlots == null)
                return false;

            if (fromIndex < 0 || fromIndex >= eqSlots.Length)
                return false;

            var eq = eqSlots[fromIndex];

            if (eq == null || eq.item == null)
                return false;

            // 🔹 STACK PRIMERO (misma durability)
            foreach (var m in metaSlots)
            {
                if (m.item != eq.item || m.durability != eq.durability)
                    continue;

                int space = eq.item.maxStack - m.amount;
                int add = Mathf.Min(space, eq.amount);

                m.amount += add;
                eq.amount -= add;

                if (eq.amount <= 0)
                    break;
            }

            // 🔹 SLOT VACÍO
            foreach (var m in metaSlots)
            {
                if (eq.amount <= 0)
                    break;

                if (m.IsEmpty())
                {
                    m.item = eq.item;
                    m.amount = eq.amount;
                    m.durability = eq.durability;
                    eq.amount = 0;
                }
            }

            // 🔥 limpiar si ya no queda nada
            if (eq.amount <= 0)
            {
                EquipmentSystem.Instance.Unequip(fromIndex);
            }

            meta.NotifyInventoryChanged();
            return true;
        }

        // =========================
        // ARMOR → BAG
        // =========================
        if (source == DragSource.Armor && target == DragSource.Bag)
        {
            var eq = EquipmentSystem.Instance.equipmentSlots[fromIndex];
            var bag = meta.bagSlots;

            if (eq == null || eq.item == null)
                return false;

            foreach (var b in bag)
            {
                if (b.IsEmpty())
                {
                    b.item = eq.item;
                    b.amount = 1;
                    b.durability = eq.durability;

                    EquipmentSystem.Instance.Unequip(fromIndex);

                    meta.NotifyInventoryChanged();
                    return true;
                }
            }

            return false;
        }

        if (source == DragSource.Armor)
        {
            var eqSlots = EquipmentSystem.Instance?.equipmentSlots;

            if (eqSlots == null || fromIndex < 0 || fromIndex >= eqSlots.Length)
                return false;
        }

        return false;
    }
}