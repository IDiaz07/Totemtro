using UnityEngine;

public static class InventoryTransferSystem
{
    public static void MoveAmount(
        DragSource source,
        int fromIndex,
        bool toMeta,
        int toIndex,
        int amount
    )
    {
        var meta = MetaInventory.Instance;
        var loadout = RunLoadoutSystem.Instance;

        // 🔒 Seguridad básica
        if (meta == null || loadout == null)
            return;

        if (meta.slots == null || loadout.loadoutSlots == null)
            return;

        InventorySlot[] fromArray =
            source == DragSource.Meta ? meta.slots : loadout.loadoutSlots;

        InventorySlot[] toArray =
            toMeta ? meta.slots : loadout.loadoutSlots;

        if (fromArray == null || toArray == null)
            return;

        if (fromIndex < 0 || fromIndex >= fromArray.Length)
            return;

        if (toIndex < 0 || toIndex >= toArray.Length)
            return;

        var fromSlot = fromArray[fromIndex];
        var toSlot = toArray[toIndex];

        if (fromSlot == null || fromSlot.item == null)
            return;

        if (amount <= 0)
            return;

        int moveAmount = Mathf.Min(amount, fromSlot.amount);

        // 🔥 MERGE
        if (toSlot.item == fromSlot.item &&
            toSlot.amount < fromSlot.item.maxStack)
        {
            int space = fromSlot.item.maxStack - toSlot.amount;
            int add = Mathf.Min(space, moveAmount);

            toSlot.amount += add;
            fromSlot.amount -= add;
        }
        // 🔥 EMPTY SLOT
        else if (toSlot.item == null)
        {
            toSlot.item = fromSlot.item;
            toSlot.amount = moveAmount;
            fromSlot.amount -= moveAmount;
        }
        // 🔥 SWAP (solo si mueves todo)
        else if (moveAmount == fromSlot.amount)
        {
            var tempItem = toSlot.item;
            var tempAmount = toSlot.amount;

            toSlot.item = fromSlot.item;
            toSlot.amount = fromSlot.amount;

            fromSlot.item = tempItem;
            fromSlot.amount = tempAmount;

            meta.NotifyInventoryChanged();
            loadout.NotifyLoadoutChanged();
            return;
        }

        if (fromSlot.amount <= 0)
            fromSlot.Clear();

        meta.NotifyInventoryChanged();
        loadout.NotifyLoadoutChanged();
    }

    public static void MoveFullStack(bool fromMeta, int index)
    {
        var meta = MetaInventory.Instance;
        var loadout = RunLoadoutSystem.Instance;

        if (meta == null || loadout == null)
            return;

        if (meta.slots == null || loadout.loadoutSlots == null)
            return;

        if (fromMeta)
        {
            if (index < 0 || index >= meta.slots.Length)
                return;

            var slot = meta.slots[index];
            if (slot == null || slot.item == null)
                return;

            for (int i = 0; i < loadout.loadoutSlots.Length; i++)
            {
                var target = loadout.loadoutSlots[i];

                if (target.item == null ||
                    target.item == slot.item)
                {
                    MoveAmount(DragSource.Meta, index, false, i, slot.amount);
                    return;
                }
            }
        }
        else
        {
            if (index < 0 || index >= loadout.loadoutSlots.Length)
                return;

            var slot = loadout.loadoutSlots[index];
            if (slot == null || slot.item == null)
                return;

            for (int i = 0; i < meta.slots.Length; i++)
            {
                var target = meta.slots[i];

                if (target.item == null ||
                    target.item == slot.item)
                {
                    MoveAmount(DragSource.Loadout, index, true, i, slot.amount);
                    return;
                }
            }
        }
    }
}