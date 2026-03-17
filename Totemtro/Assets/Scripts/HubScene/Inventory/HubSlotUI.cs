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

    public enum ArmorSlotType { Helmet = 0, Chest = 1, Pants = 2, Boots = 3 }
    public ArmorSlotType armorSlot = ArmorSlotType.Helmet;

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
                return MetaInventory.Instance?.armorSlots?[(int)armorSlot];

            case DragSource.Loadout:
                return RunLoadoutSystem.Instance?.loadoutSlots?[slotIndex];
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

        if (!Input.GetKey(KeyCode.LeftShift) &&
            !Input.GetKey(KeyCode.RightShift))
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
        // COMBAT SCENE
        // ===============================
        if (scene.Contains("CombatScene"))
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

}