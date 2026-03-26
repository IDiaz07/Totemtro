using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class HubSlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IDropHandler,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Image icon;
    public TMP_Text amountText;

    public DragSource slotType = DragSource.Meta;
    public int slotIndex;

    public EquipmentSlotType armorSlot = EquipmentSlotType.Helmet;

    ItemData currentItem;
    int currentAmount;

    Coroutine delayedSubscribeCoroutine;

    IEnumerator Start()
    {
        yield return null;
        Refresh();
    }

    public int EffectiveIndex =>
        slotType == DragSource.Armor ? (int)armorSlot : slotIndex;

    // =====================================================
    // REFRESH UI
    // =====================================================

    public void Refresh()
    {
        InventorySlot slot = GetSlot();

        if (slot == null || slot.item == null)
        {
            currentItem = null;
            currentAmount = 0;

            if (icon != null)
            {
                icon.sprite = null;
                icon.color = Color.white; // ← nunca transparente
                icon.enabled = false;     // ← la visibilidad la controla enabled
            }

            if (amountText != null)
                amountText.text = "";

            return;
        }

        currentItem = slot.item;
        currentAmount = slot.amount;

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = currentItem.icon;
            icon.color = Color.white;
        }

        if (amountText != null)
            amountText.text = (slotType != DragSource.Armor && currentAmount > 1)
                ? currentAmount.ToString()
                : "";
    }

    // =====================================================
    // SLOT RESOLUTION
    // =====================================================

    InventorySlot GetSlot()
    {
        switch (slotType)
        {
            case DragSource.Meta:
                return MetaInventory.Instance?.slots?[slotIndex];

            case DragSource.Bag:
                return MetaInventory.Instance?.bagSlots?[slotIndex];

            case DragSource.Armor:
                if (EquipmentSystem.Instance == null)
                    return null;

                int index = EquipmentSystem.Instance.GetIndex(armorSlot);

                if (index < 0 || index >= EquipmentSystem.Instance.equipmentSlots.Length)
                    return null;

                return EquipmentSystem.Instance.equipmentSlots[index];

            case DragSource.Loadout:
                return RunLoadoutSystem.Instance?.loadoutSlots?[slotIndex];

            case DragSource.Chest:
                return ChestUI.CurrentChest?.slots?[slotIndex];

        }

        return null;
    }

    // =====================================================
    // DRAG
    // =====================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = GetSlot();

        if (slot == null || slot.item == null)
            return;

        if (MetaDragUI.Instance == null)
            return;

        MetaDragUI.Instance.Show(
            slot.item,
            slot.amount,
            slotType,
            EffectiveIndex
        );
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;

        if (drag == null || !drag.IsDragging)
            return;

        // ===============================
        // ARMOR SLOT
        // ===============================
        if (slotType == DragSource.Armor)
        {
            if (EquipmentSystem.Instance == null)
                return;

            int index = EquipmentSystem.Instance.GetIndex(armorSlot);

            if (index < 0 || index >= EquipmentSystem.Instance.equipmentSlots.Length)
                return;

            ItemData item = drag.draggedItem;

            // 👉 EQUIPAR
            if (item != null && item.itemType == ItemType.Equipment)
            {
                // validar tipo correcto
                if (item.equipmentSlotType != armorSlot)
                {
                    Debug.Log("Item slot: " + item.equipmentSlotType + " = " + (int)item.equipmentSlotType);
                    Debug.Log("UI slot: " + armorSlot + " = " + (int)armorSlot);
                    return;
                }

                // 🔥 EQUIPAR
                EquipmentSystem.Instance.EquipItem(item, index);

                // 🔥 eliminar del origen
                var sourceSlots = GetSourceSlots(drag.source);

                if (sourceSlots != null &&
                    drag.sourceIndex >= 0 &&
                    drag.sourceIndex < sourceSlots.Length)
                {
                    sourceSlots[drag.sourceIndex].Clear();
                }

                MetaInventory.Instance?.NotifyInventoryChanged();

                drag.Hide();
                return;
            }

            // 👉 DESEQUIPAR (drag desde armor a otro lado)
            var eq = EquipmentSystem.Instance.equipmentSlots[index];

            if (eq == null || eq.item == null)
                return;

            var bag = MetaInventory.Instance?.bagSlots;

            if (bag == null)
                return;

            // meter en bag
            foreach (var b in bag)
            {
                if (b.IsEmpty())
                {
                    b.item = eq.item;
                    b.amount = 1;
                    break;
                }
            }

            EquipmentSystem.Instance.Unequip(index);

            MetaInventory.Instance.NotifyInventoryChanged();

            drag.Hide();
            return;
        }

        // ===============================
        // NORMAL TRANSFER
        // ===============================
        InventoryTransferSystem.MoveAmount(
            drag.source,
            drag.sourceIndex,
            slotType,
            EffectiveIndex,
            drag.draggedAmount
        );

        drag.Hide();
    }

    // =====================================================
    // SHIFT CLICK TRANSFERS
    // =====================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (!shift)
            return;

        var slot = GetSlot();

        if (slot == null || slot.IsEmpty())
            return;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // ===============================
        // HUB SCENE
        // ===============================
        if (scene.Contains("HubScene"))
        {
            if (slotType == DragSource.Meta)
            {
                InventoryTransferSystem.MoveFullStack(
                    DragSource.Meta,
                    EffectiveIndex,
                    DragSource.Bag
                );
                return;
            }

            if (slotType == DragSource.Bag)
            {
                InventoryTransferSystem.MoveFullStack(
                    DragSource.Bag,
                    EffectiveIndex,
                    DragSource.Meta
                );
                return;
            }

            if (slotType == DragSource.Armor)
            {
                InventoryTransferSystem.MoveFullStack(
                    DragSource.Armor,
                    EffectiveIndex,
                    DragSource.Bag
                );
                return;
            }
        }

        // ===============================
        // INVENTORY → ACTION BAR (WHEN INVENTORY OPEN)
        // ===============================
        if (InventoryController.Instance != null &&
            InventoryController.Instance.IsInventoryOpen)
        {
            if (slotType == DragSource.Bag)
            {
                InventoryTransferSystem.MoveFullStack(
                    DragSource.Bag,
                    EffectiveIndex,
                    DragSource.ActionBar
                );
                return;
            }
        }

        // ===============================
        // CHEST SYSTEM (WHEN UI OPEN)
        // ===============================
        var chestUI = FindFirstObjectByType<ChestUI>(FindObjectsInactive.Include);

        if (chestUI != null && chestUI.IsOpen && ChestUI.CurrentChest != null)
        {
            // BAG → CHEST
            if (slotType == DragSource.Bag)
            {
                InventoryTransferSystem.MoveFullStack(
                    DragSource.Bag,
                    EffectiveIndex,
                    DragSource.Chest
                );
                return;
            }

            // CHEST → BAG
            if (slotType == DragSource.Chest)
            {
                InventoryTransferSystem.MoveFullStack(
                    DragSource.Chest,
                    EffectiveIndex,
                    DragSource.Bag
                );
                return;
            }
        }
    }

    // =====================================================
    // DRAG SPREAD SUPPORT
    // =====================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;

        if (drag != null && drag.IsDragging)
            drag.RegisterHoveredSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData) { }

    // =====================================================
    // SUBSCRIBE EVENTS
    // =====================================================

    void OnEnable()
    {
        if (slotType == DragSource.Chest)
        {
            if (ChestUI.CurrentChest != null)
            {
                ChestUI.CurrentChest.onChestChanged += Refresh;
                StartCoroutine(DelayedRefresh());
            }
        }
        

        if (slotType == DragSource.Loadout)
        {
            if (RunLoadoutSystem.Instance != null)
            {
                RunLoadoutSystem.Instance.onLoadoutChanged += Refresh;
                StartCoroutine(DelayedRefresh());
            }
            else
            {
                delayedSubscribeCoroutine =
                    StartCoroutine(WaitForRunLoadoutAndSubscribe());
            }
        }
        else
        {
            if (MetaInventory.Instance != null)
            {
                MetaInventory.Instance.onInventoryChanged += Refresh;
                StartCoroutine(DelayedRefresh());
            }
            else
            {
                delayedSubscribeCoroutine =
                    StartCoroutine(WaitForMetaAndSubscribe());
            }
        }
    }

    void OnDisable()
    {
        if (slotType == DragSource.Chest)
        {
            if (ChestUI.CurrentChest != null)
                ChestUI.CurrentChest.onChestChanged -= Refresh;
        }

        if (delayedSubscribeCoroutine != null)
            StopCoroutine(delayedSubscribeCoroutine);

        if (slotType == DragSource.Loadout)
        {
            if (RunLoadoutSystem.Instance != null)
                RunLoadoutSystem.Instance.onLoadoutChanged -= Refresh;
        }
        else
        {
            if (MetaInventory.Instance != null)
                MetaInventory.Instance.onInventoryChanged -= Refresh;
        }
    }

    IEnumerator WaitForMetaAndSubscribe()
    {
        while (MetaInventory.Instance == null)
            yield return null;

        MetaInventory.Instance.onInventoryChanged += Refresh;
        Refresh();
    }

    IEnumerator WaitForRunLoadoutAndSubscribe()
    {
        while (RunLoadoutSystem.Instance == null)
            yield return null;

        RunLoadoutSystem.Instance.onLoadoutChanged += Refresh;
        Refresh();
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;
        Refresh();
    }

    public ItemData GetCurrentItem()
    {
        var slot = GetSlot();

        if (slot == null)
            return null;

        return slot.item;
    }

    InventorySlot[] GetSourceSlots(DragSource src)
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

}