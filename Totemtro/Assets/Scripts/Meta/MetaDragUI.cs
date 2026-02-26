using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MetaDragUI : MonoBehaviour
{
    public static MetaDragUI Instance;

    public Image icon;

    public bool IsDragging { get; private set; }

    public DragSource source;
    public int sourceIndex;

    public ItemData draggedItem;   // 🔥 NECESARIO
    public int draggedAmount;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (!IsDragging) return;

        transform.position = Input.mousePosition;

        // CLICK DERECHO → soltar 1
        if (Input.GetMouseButtonDown(1))
            DropSingle();

        // SOLTAR CLICK IZQUIERDO → cancelar
        if (Input.GetMouseButtonUp(0))
            CancelDrag();
    }

    void DropSingle()
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            var meta = r.gameObject.GetComponent<HubMetaSlotUI>();
            if (meta != null)
            {
                InventoryTransferSystem.MoveAmount(
                    source, sourceIndex, true, meta.slotIndex, 1);

                draggedAmount--;
                return;
            }

            var load = r.gameObject.GetComponent<HubLoadoutSlotUI>();
            if (load != null)
            {
                InventoryTransferSystem.MoveAmount(
                    source, sourceIndex, false, load.slotIndex, 1);

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

        Hide();
    }

    public void Hide()
    {
        IsDragging = false;
        draggedItem = null;
        draggedAmount = 0;
        source = DragSource.None;
        sourceIndex = -1;

        gameObject.SetActive(false);
    }
}