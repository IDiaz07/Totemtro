using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ActionSlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Image icon;
    public Image cooldownOverlay;
    public TMP_Text amountText;
    public TMP_Text keyText;

    int slotIndex;
    ActionBarController actionBar;

    bool isPointerOver;

    float lastClickTime;
    const float doubleClickThreshold = 0.35f;

    void Awake()
    {
        actionBar = FindFirstObjectByType<ActionBarController>();
    }

    public void Setup(int index)
    {
        slotIndex = index;

        if (keyText != null)
            keyText.text = (index + 1).ToString();
    }

    void Update()
    {
        if (actionBar == null)
            return;

        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
        {
            icon.enabled = false;
            amountText.text = "";
            cooldownOverlay.fillAmount = 0f;
            return;
        }

        icon.enabled = true;
        icon.sprite = slot.item.icon;

        amountText.text =
            slot.amount > 1 ? slot.amount.ToString() : "";

        if (slot.item.cooldown > 0)
            cooldownOverlay.fillAmount =
                slot.cooldownRemaining / slot.item.cooldown;
        else
            cooldownOverlay.fillAmount = 0f;

        if (isPointerOver &&
            Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKeyDown(KeyCode.Q))
        {
            DropStack();
        }
    }

    void DropStack()
    {
        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
            return;

        slot.Clear();

        MetaInventory.Instance.NotifyInventoryChanged();
    }

    // =====================
    // DRAG
    // =====================

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
            return;

        MetaDragUI.Instance.Show(
            slot.item,
            slot.amount,
            DragSource.ActionBar,
            slotIndex
        );
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;

        if (drag == null || !drag.IsDragging)
            return;

        drag.Hide();
    }

    // =====================
    // DROP
    // =====================

    public void OnDrop(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;

        if (drag == null || !drag.IsDragging)
            return;

        // ACTIONBAR → ACTIONBAR
        if (drag.source == DragSource.ActionBar)
        {
            int from = drag.sourceIndex;
            int to = slotIndex;

            var a = actionBar.slots[from];
            var b = actionBar.slots[to];

            actionBar.slots[from] = b;
            actionBar.slots[to] = a;

            drag.Hide();
            return;
        }

        // BAG → ACTIONBAR
        if (drag.source == DragSource.Bag)
        {
            var meta = MetaInventory.Instance;

            var bagSlot = meta.bagSlots[drag.sourceIndex];

            if (bagSlot.IsEmpty())
                return;

            var actionSlot = actionBar.slots[slotIndex];

            actionSlot.item = bagSlot.item;
            actionSlot.amount = bagSlot.amount;

            bagSlot.Clear();

            meta.NotifyInventoryChanged();

            drag.Hide();
            return;
        }
    }

    // =====================
    // CLICK
    // =====================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            QuickMoveToInventory();
            return;
        }


        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
            return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            QuickMoveToInventory();
            return;
        }

        if (eventData.button ==
            PointerEventData.InputButton.Right)
        {
            UseItem();
            return;
        }

        if (eventData.button ==
            PointerEventData.InputButton.Left)
        {
            if (Time.unscaledTime - lastClickTime <
                doubleClickThreshold)
                StackAllItems();

            lastClickTime = Time.unscaledTime;
        }
    }

    void QuickMoveToInventory()
    {
        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
            return;

        MetaInventory meta = MetaInventory.Instance;

        if (meta.AddItem(slot.item, slot.amount))
            slot.Clear();

        meta.NotifyInventoryChanged();
    }

    void UseItem()
    {
        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
            return;

        var hero = FindFirstObjectByType<HeroController>();

        if (hero == null)
            return;

        if (slot.item.ability == null)
            return;

        bool activated =
            slot.item.ability.TryActivate(hero.gameObject);

        if (!activated)
            return;

        slot.amount--;

        if (slot.amount <= 0)
            slot.Clear();

        MetaInventory.Instance.NotifyInventoryChanged();
    }

    void StackAllItems()
    {
        var slot = actionBar.slots[slotIndex];

        if (slot == null || slot.IsEmpty())
            return;

        MetaInventory meta = MetaInventory.Instance;

        foreach (var s in meta.slots)
        {
            if (s.IsEmpty())
                continue;

            if (s.item != slot.item)
                continue;

            int space =
                slot.item.maxStack - slot.amount;

            if (space <= 0)
                break;

            int move =
                Mathf.Min(space, s.amount);

            slot.amount += move;
            s.amount -= move;

            if (s.amount <= 0)
                s.Clear();
        }

        meta.NotifyInventoryChanged();
    }

    // =====================
    // POINTER
    // =====================

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

    public ItemData GetCurrentItem()
    {
        if (actionBar == null)
            return null;

        var slot = actionBar.slots[slotIndex];

        if (slot == null)
            return null;

        return slot.item;
    }
}