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

    [HideInInspector]
    public int slotIndex;

    ItemData currentItem;
    int currentAmount;

    void Awake()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged += Refresh;
    }

    void OnDestroy()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged -= Refresh;
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        // Diagnóstico rápido: comprobar referencias del prefab
        if (icon == null)
        {
            Debug.LogError($"HubMetaSlotUI: 'icon' no asignado en prefab (slotIndex={slotIndex})");
            return;
        }

        if (amountText == null)
        {
            Debug.LogError($"HubMetaSlotUI: 'amountText' no asignado en prefab (slotIndex={slotIndex})");
            return;
        }

        var inventory = MetaInventory.Instance;

        if (inventory == null)
            return;

        if (inventory.slots == null)
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

        // Comprobar que el ItemData tiene sprite
        if (currentItem.icon == null)
        {
            Debug.LogWarning($"HubMetaSlotUI: item '{currentItem.itemName}' (id={currentItem.itemID}) tiene icon NULL (slot {slotIndex})");
        }

        icon.enabled = true;
        icon.sprite = currentItem.icon;

        amountText.text =
            currentAmount > 1 ?
            currentAmount.ToString() :
            "";
    }

    // ==========================
    // DRAG
    // ==========================

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

        if (drag == null || !drag.IsDragging)
            return;

        InventoryTransferSystem.MoveAmount(
            drag.source,
            drag.sourceIndex,
            DragSource.Meta,
            slotIndex,
            drag.draggedAmount
        );

        drag.Hide();
    }

    // ==========================
    // QUICK TRANSFER
    // ==========================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            InventoryTransferSystem.MoveFullStack(
                DragSource.Meta,
                slotIndex,
                DragSource.Loadout
            );
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
                    DragSource.Loadout,
                    target,
                    half
                );
            }
        }
    }

    int FindFirstFreeLoadoutSlot()
    {
        var loadout = RunLoadoutSystem.Instance;

        if (loadout == null)
            return -1;

        if (loadout.loadoutSlots == null)
            return -1;

        for (int i = 0; i < loadout.loadoutSlots.Length; i++)
        {
            var slot = loadout.loadoutSlots[i];

            if (slot.item == null || slot.item == currentItem)
                return i;
        }

        return -1;
    }
}