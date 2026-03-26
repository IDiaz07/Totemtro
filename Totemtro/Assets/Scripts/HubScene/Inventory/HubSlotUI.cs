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

    [Header("Durability UI")]
    public GameObject durabilityBarRoot;
    public Image durabilityFill;

    Coroutine delayedSubscribeCoroutine;

    IEnumerator Start()
    {
        yield return null;
        yield return null;
        Refresh();
    }

    public int EffectiveIndex =>
    slotType == DragSource.Armor
        ? EquipmentSystem.Instance.GetIndex(armorSlot)
        : slotIndex;

    // =====================================================
    // REFRESH UI
    // =====================================================

    public void Refresh()
    {
        InventorySlot slot = GetSlot();

        if (slot != null)
            slot.EnsureDurability();

        if (slot == null || slot.item == null)
        {
            currentItem = null;
            currentAmount = 0;

            if (icon != null)
            {
                icon.sprite = null;
                icon.color = Color.white;
                icon.enabled = false;
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
        // ===============================
        // DURABILITY UI (FIX COMPLETO)
        // ===============================
        if (durabilityBarRoot != null && durabilityFill != null)
        {
            // RESET SIEMPRE (CLAVE DEL BUG)
            durabilityBarRoot.SetActive(false);

            if (slot == null || slot.item == null)
                return;

            // SOLO EQUIPMENT
            if (slot.item.itemType != ItemType.Equipment)
                return;

            float max = slot.item.maxDurability;

            if (max <= 0)
                return;

            float current = slot.durability;

            // ITEM NUEVO → NO MOSTRAR
            if (current >= max)
                return;

            float ratio = current / max;

            // SI ESTÁ ROTO → NO MOSTRAR (se elimina igual)
            if (ratio <= 0f)
                return;

            // ACTIVAR UI
            durabilityBarRoot.SetActive(true);
            durabilityFill.fillAmount = ratio;

            // COLOR
            if (ratio > 0.5f)
                durabilityFill.color = Color.green;
            else if (ratio > 0.15f)
                durabilityFill.color = Color.yellow;
            else
                durabilityFill.color = Color.red;
        }
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

            // =====================================================
            // 👉 EQUIPAR
            // =====================================================
            if (item != null && item.itemType == ItemType.Equipment)
            {
                // validar tipo correcto
                if (item.equipmentSlotType != armorSlot)
                {
                    Debug.Log("Item slot: " + item.equipmentSlotType + " = " + (int)item.equipmentSlotType);
                    Debug.Log("UI slot: " + armorSlot + " = " + (int)armorSlot);
                    return;
                }

                var sourceSlots = GetSourceSlots(drag.source);

                if (sourceSlots == null ||
                    drag.sourceIndex < 0 ||
                    drag.sourceIndex >= sourceSlots.Length)
                    return;

                var sourceSlot = sourceSlots[drag.sourceIndex];

                if (sourceSlot == null || sourceSlot.item == null)
                    return;

                // 🔥 EQUIPAR (CON DURABILITY)
                EquipmentSystem.Instance.EquipItem(sourceSlot, index);

                // 🔥 limpiar origen
                sourceSlot.Clear();

                MetaInventory.Instance?.NotifyInventoryChanged();

                drag.Hide();
                return;
            }

            // =====================================================
            // 👉 DESEQUIPAR (drag desde armor)
            // =====================================================
            var eq = EquipmentSystem.Instance.equipmentSlots[index];

            if (eq == null || eq.item == null)
                return;

            var bag = MetaInventory.Instance?.bagSlots;

            if (bag == null)
                return;

            // 🔥 meter en bag CONSERVANDO DURABILITY
            foreach (var b in bag)
            {
                if (b.IsEmpty())
                {
                    b.item = eq.item;
                    b.amount = 1;
                    b.durability = eq.durability; // 🔥 CLAVE
                    break;
                }
            }

            // 🔥 quitar del equipo
            EquipmentSystem.Instance.Unequip(index);

            MetaInventory.Instance.NotifyInventoryChanged();

            drag.Hide();
            return;
        }

        // ===============================
        // 👉 UNEQUIP → DRAG A OTRO SLOT
        // ===============================
        if (drag.source == DragSource.Armor && slotType != DragSource.Armor)
        {
            int armorIndex = drag.sourceIndex;

            var eqSlots = EquipmentSystem.Instance.equipmentSlots;
            var sourceSlot = eqSlots[armorIndex];

            if (sourceSlot == null || sourceSlot.item == null)
                return;

            var targetSlots = GetSourceSlots(slotType);

            if (targetSlots == null)
                return;

            var targetSlot = targetSlots[EffectiveIndex];

            // 🔹 SI VACÍO → MOVER
            if (targetSlot.IsEmpty())
            {
                targetSlot.item = sourceSlot.item;
                targetSlot.amount = 1;
                targetSlot.durability = sourceSlot.durability;

                EquipmentSystem.Instance.Unequip(armorIndex);

                MetaInventory.Instance.NotifyInventoryChanged();
                drag.Hide();
                return;
            }

            // 🔹 SI MISMO ITEM → STACK
            if (targetSlot.item == sourceSlot.item &&
                targetSlot.durability == sourceSlot.durability)
            {
                targetSlot.amount += 1;

                EquipmentSystem.Instance.Unequip(armorIndex);

                MetaInventory.Instance.NotifyInventoryChanged();
                drag.Hide();
                return;
            }

            // 🔹 SI OCUPADO → SWAP
            var tempItem = targetSlot.item;
            var tempDurability = targetSlot.durability;

            targetSlot.item = sourceSlot.item;
            targetSlot.amount = 1;
            targetSlot.durability = sourceSlot.durability;

            sourceSlot.item = tempItem;
            sourceSlot.amount = 1;
            sourceSlot.durability = tempDurability;

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
            // ===============================
            // ARMOR → META (PRIORIDAD HUB)
            // ===============================
            if (slotType == DragSource.Armor)
            {
                bool success = false;

                // 👉 1. INVENTORY
                success = InventoryTransferSystem.MoveFullStack(
                    DragSource.Armor,
                    EffectiveIndex,
                    DragSource.Meta
                );

                // 👉 2. BAG
                if (!success)
                {
                    success = InventoryTransferSystem.MoveFullStack(
                        DragSource.Armor,
                        EffectiveIndex,
                        DragSource.Bag
                    );
                }

                // 👉 3. SHAKE
                if (!success)
                {
                    var shake = FindFirstObjectByType<UIShake>();
                    if (shake != null) shake.Play();
                }

                return;
            }

            // ===============================
            // AUTO EQUIP (META / BAG)
            // ===============================
            if (slotType == DragSource.Meta || slotType == DragSource.Bag)
            {
                if (slot.item.itemType == ItemType.Equipment)
                {
                    bool equipped = TryAutoEquip(slot);

                    if (equipped)
                        return;
                }
            }

            // ===============================
            // NORMAL TRANSFERS
            // ===============================
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
        }

        // ===============================
        // ⚔️ COMBAT SCENE
        // ===============================
        if (scene.Contains("CombatScene"))
        {
            if (slotType == DragSource.Bag)
            {
                if (slot.item.itemType == ItemType.Equipment)
                {
                    bool equipped = TryAutoEquip(slot);

                    if (!equipped)
                    {
                        // ❌ SLOT OCUPADO → SHAKE
                        var shake = FindFirstObjectByType<UIShake>();

                        if (shake != null)
                            shake.Play();
                    }

                    return;
                }
            }
        }

        // ===============================
        // BAG → ACTION BAR (WHEN INVENTORY OPEN)
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
        // ARMOR → BAG (SHIFT)
        // ===============================
        if (slotType == DragSource.Armor)
        {
            InventoryTransferSystem.MoveFullStack(
                DragSource.Armor,
                EffectiveIndex,
                DragSource.Bag
            );
            return;
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
        var slot = GetSlot();

        if (slot == null || slot.item == null)
            return;

        ItemTooltipUI.Instance.Show(
            slot.item,
            slot,
            transform as RectTransform
        );
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

        if (slotType == DragSource.Armor)
        {
            if (EquipmentSystem.Instance != null)
                EquipmentSystem.Instance.onEquipmentChanged += Refresh;

            // 🔥 FIX: forzar refresh después de suscribirse
            StartCoroutine(ForceRefreshNextFrame());
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

        if (slotType == DragSource.Armor)
        {
            if (EquipmentSystem.Instance != null)
                EquipmentSystem.Instance.onEquipmentChanged -= Refresh;
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

    public InventorySlot GetSlotForTooltip()
    {
        return GetSlot();
    }

    bool TryAutoEquip(InventorySlot sourceSlot)
    {
        if (sourceSlot == null || sourceSlot.item == null)
            return false;

        if (sourceSlot.item.itemType != ItemType.Equipment)
            return false;

        var eq = EquipmentSystem.Instance;

        if (eq == null)
            return false;

        int index = eq.GetIndex(sourceSlot.item.equipmentSlotType);

        if (index < 0)
            return false;

        var targetSlot = eq.equipmentSlots[index];

        // 🟢 SLOT VACÍO → EQUIPAR
        if (targetSlot.item == null)
        {
            eq.EquipItem(sourceSlot, index);
            sourceSlot.Clear();

            MetaInventory.Instance?.NotifyInventoryChanged();
            return true;
        }

        return false; // ocupado
    }

    IEnumerator ForceRefreshNextFrame()
    {
        yield return null; // espera 1 frame
        Refresh();
    }
}