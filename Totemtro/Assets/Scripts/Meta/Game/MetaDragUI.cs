using UnityEngine.EventSystems;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetaDragUI : MonoBehaviour
{
    public static MetaDragUI Instance;
    public Image icon;
    public bool IsDragging { get; private set; }
    public DragSource source;
    public int sourceIndex;
    public ItemData draggedItem;
    public int draggedAmount;

    public bool rightDragMode = false;
    public bool leftSpreadMode = false;

    public List<HubSlotUI> hoveredSlots = new List<HubSlotUI>();

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (!IsDragging) return;
        transform.position = Input.mousePosition;
        if (Input.GetMouseButtonDown(1)) DropSingle();
        if (Input.GetMouseButtonUp(0)) CancelDrag();
    }

    public void RegisterHoveredSlot(HubSlotUI slot)
    {
        if (!hoveredSlots.Contains(slot))
            hoveredSlots.Add(slot);
    }

    public void ClearHoveredSlots()
    {
        hoveredSlots.Clear();
    }

    void DropSingle()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            var hub = r.gameObject.GetComponent<HubSlotUI>();
            if (hub != null)
            {
                // Usar el tipo de slot del componente genérico
                if (hub.slotType == DragSource.Meta)
                {
                    InventoryTransferSystem.MoveAmount(
                        source,
                        sourceIndex,
                        DragSource.Meta,
                        hub.slotIndex,
                        1
                    );

                    draggedAmount--;
                    return;
                }
                else if (hub.slotType == DragSource.Loadout)
                {
                    InventoryTransferSystem.MoveAmount(
                        source,
                        sourceIndex,
                        DragSource.Loadout,
                        hub.slotIndex,
                        1
                    );

                    draggedAmount--;
                    return;
                }
                else if (hub.slotType == DragSource.Bag)
                {
                    InventoryTransferSystem.MoveAmount(
                        source,
                        sourceIndex,
                        DragSource.Bag,
                        hub.slotIndex,
                        1
                    );

                    draggedAmount--;
                    return;
                }
                else if (hub.slotType == DragSource.Armor)
                {
                    // CORRECCIÓN: usar el índice efectivo (usa armorSlot internamente)
                    InventoryTransferSystem.MoveAmount(
                        source,
                        sourceIndex,
                        DragSource.Armor,
                        hub.EffectiveIndex,
                        1
                    );

                    draggedAmount--;
                    return;
                }
            }

            // Compatibilidad con otros handlers existentes (si hubiera)
            var meta = r.gameObject.GetComponent<HubMetaSlotUI>();
            if (meta != null)
            {
                InventoryTransferSystem.MoveAmount(
                    source,
                    sourceIndex,
                    DragSource.Meta,
                    meta.slotIndex,
                    1
                );

                draggedAmount--;
                return;
            }
        }
    }

    public void Show(ItemData item, int amount, DragSource src, int index)
    {
        draggedItem = item;
        draggedAmount = amount;
        source = src;
        sourceIndex = index;

        icon.sprite = item.icon;
        icon.color = Color.white;

        IsDragging = true;
        gameObject.SetActive(true);

        rightDragMode = Input.GetMouseButton(1);
        leftSpreadMode = Input.GetMouseButton(0);
    }

    void CancelDrag()
    {
        var meta = MetaInventory.Instance;
        var loadout = RunLoadoutSystem.Instance;

        if (source == DragSource.Meta)
        {
            meta.slots[sourceIndex].item = draggedItem;
            meta.slots[sourceIndex].amount = draggedAmount;
            meta.NotifyInventoryChanged();
        }
        else if (source == DragSource.Loadout)
        {
            loadout.loadoutSlots[sourceIndex].item = draggedItem;
            loadout.loadoutSlots[sourceIndex].amount = draggedAmount;
            loadout.NotifyLoadoutChanged();
        }
        else if (source == DragSource.Bag)
        {
            if (meta != null && meta.bagSlots != null && sourceIndex >= 0 && sourceIndex < meta.bagSlots.Length)
            {
                meta.bagSlots[sourceIndex].item = draggedItem;
                meta.bagSlots[sourceIndex].amount = draggedAmount;
                meta.NotifyInventoryChanged();
            }
        }
        else if (source == DragSource.Armor)
        {
            if (meta != null && meta.armorSlots != null && sourceIndex >= 0 && sourceIndex < meta.armorSlots.Length)
            {
                meta.armorSlots[sourceIndex].item = draggedItem;
                meta.armorSlots[sourceIndex].amount = draggedAmount;
                meta.NotifyInventoryChanged();
            }
        }

        Hide();
    }

    void ApplySpread()
    {
        if (hoveredSlots.Count == 0)
            return;

        if (rightDragMode)
        {
            // 1 item por slot
            foreach (var slot in hoveredSlots)
            {
                if (draggedAmount <= 0) break;

                InventoryTransferSystem.MoveAmount(
                    source,
                    sourceIndex,
                    slot.slotType,
                    slot.EffectiveIndex,
                    1
                );

                draggedAmount--;
            }
        }
        else if (leftSpreadMode)
        {
            int perSlot = draggedAmount / hoveredSlots.Count;

            if (perSlot <= 0) perSlot = 1;

            foreach (var slot in hoveredSlots)
            {
                if (draggedAmount <= 0) break;

                InventoryTransferSystem.MoveAmount(
                    source,
                    sourceIndex,
                    slot.slotType,
                    slot.EffectiveIndex,
                    perSlot
                );

                draggedAmount -= perSlot;
            }
        }

        ClearHoveredSlots();
    }

    public void Hide()
    {
        IsDragging = false;
        draggedItem = null;
        draggedAmount = 0;
        source = DragSource.None;
        sourceIndex = -1;
        gameObject.SetActive(false);
        ApplySpread();
    }
}