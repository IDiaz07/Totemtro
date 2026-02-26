using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HubMetaSlotUI : MonoBehaviour,
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
        var inventory = MetaInventory.Instance;

        if (inventory == null || inventory.slots == null)
            return;

        if (slotIndex < 0 || slotIndex >= inventory.slots.Length)
            return;

        var slot = inventory.slots[slotIndex];

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
        if (currentItem == null)
            return;

        if (MetaDragUI.Instance == null)
            return;

        MetaDragUI.Instance.Show(
            currentItem,
            currentAmount,
            DragSource.Meta,
            slotIndex
        );
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;

        if (drag == null || !drag.IsDragging || drag.draggedItem == null)
            return;

        InventoryTransferSystem.MoveAmount(
            drag.source,
            drag.sourceIndex,
            true,
            slotIndex,
            drag.draggedAmount
        );

        drag.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            InventoryTransferSystem.MoveFullStack(true, slotIndex);
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            int half = Mathf.CeilToInt(currentAmount / 2f);
            int target = FindFirstFreeLoadoutSlot();

            if (target >= 0)
            {
                InventoryTransferSystem.MoveAmount(
                    DragSource.Meta,
                    slotIndex,
                    false,
                    target,
                    half
                );
            }
        }
    }

    int FindFirstFreeLoadoutSlot()
    {
        var loadout = RunLoadoutSystem.Instance;

        if (loadout == null || loadout.loadoutSlots == null)
            return -1;

        for (int i = 0; i < loadout.loadoutSlots.Length; i++)
        {
            if (loadout.loadoutSlots[i].item == null ||
                loadout.loadoutSlots[i].item == currentItem)
                return i;
        }

        return -1;
    }

    void OnEnable()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged += Refresh;
    }

    void OnDisable()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged -= Refresh;
    }
}