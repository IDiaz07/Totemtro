using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HubLoadoutSlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IDropHandler,
    IPointerClickHandler
{
    public Image icon;
    public TMP_Text amountText;
    public int slotIndex;

    ItemData currentItem;
    int currentAmount;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        var slot = RunLoadoutSystem.Instance.loadoutSlots[slotIndex];

        currentItem = slot.item;
        currentAmount = slot.amount;

        if (currentItem == null)
        {
            icon.enabled = false;
            amountText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = currentItem.icon;
        amountText.text = currentAmount > 1 ? currentAmount.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BEGIN DRAG META SLOT " + slotIndex);

        if (currentItem == null) return;

        MetaDragUI.Instance.Show(
            currentItem,
            currentAmount,
            DragSource.Loadout,
            slotIndex
        );
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;

        if (drag == null)
            return;

        if (!drag.IsDragging)
            return;

        if (drag.draggedItem == null)
            return;

        if (RunLoadoutSystem.Instance == null)
            return;

        InventoryTransferSystem.MoveAmount(
            drag.source,
            drag.sourceIndex,
            false,
            slotIndex,
            drag.draggedAmount
        );

        drag.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // SHIFT
        if (Input.GetKey(KeyCode.LeftShift))
            InventoryTransferSystem.MoveFullStack(false, slotIndex);

        // CTRL
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            int half = Mathf.CeilToInt(currentAmount / 2f);

            InventoryTransferSystem.MoveAmount(
                DragSource.Loadout,
                slotIndex,
                true,
                FindFirstFreeMetaSlot(),
                half
            );
        }
    }

    int FindFirstFreeMetaSlot()
    {
        var meta = MetaInventory.Instance;

        for (int i = 0; i < meta.slots.Length; i++)
        {
            if (meta.slots[i].item == null ||
                meta.slots[i].item == currentItem)
                return i;
        }

        return -1;
    }

    void OnEnable()
    {
        if (RunLoadoutSystem.Instance != null)
            RunLoadoutSystem.Instance.onLoadoutChanged += Refresh;
    }

    void OnDisable()
    {
        if (RunLoadoutSystem.Instance != null)
            RunLoadoutSystem.Instance.onLoadoutChanged -= Refresh;
    }
}