using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class ActionSlotUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    public Image icon;
    public Image cooldownOverlay;
    public TMP_Text amountText;
    public TMP_Text keyText;

    int slotIndex;
    ActionBarController actionBar;

    void Awake()
    {
        actionBar = FindFirstObjectByType<ActionBarController>();
    }

    public void Setup(int index)
    {
        slotIndex = index;
        keyText.text = (index + 1).ToString();
    }

    void Update()
    {
        var slot = actionBar.slots[slotIndex];

        if (slot.IsEmpty())
        {
            icon.enabled = false;
            amountText.text = "";
            cooldownOverlay.fillAmount = 0f;
            return;
        }

        icon.enabled = true;
        icon.sprite = slot.item.icon;
        amountText.text = slot.amount > 1 ? slot.amount.ToString() : "";

        if (slot.item.cooldown > 0f)
            cooldownOverlay.fillAmount =
                slot.cooldownRemaining / slot.item.cooldown;
        else
            cooldownOverlay.fillAmount = 0f;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = DragItemManager.Instance;

        if (!drag.IsDragging)
            return;

        // 🔥 Si viene del inventario
        if (drag.sourceType == DragSourceType.Inventory)
        {
            RunInventory inventory =
                FindFirstObjectByType<RunInventory>();

            var slot = inventory.slots[drag.sourceIndex];

            // ❌ SOLO CONSUMIBLES
            if (slot.item.itemType != ItemType.Consumable)
            {
                StartCoroutine(Shake());
                return;
            }

            actionBar.AssignToSlot(drag.sourceIndex, slotIndex);
        }

        // 🔥 Si viene del ActionBar
        if (drag.sourceType == DragSourceType.ActionBar)
        {
            var other = actionBar.slots[drag.sourceIndex];
            var current = actionBar.slots[slotIndex];

            actionBar.slots[slotIndex] = other;
            actionBar.slots[drag.sourceIndex] = current;
        }

        drag.ClearDrag();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = actionBar.slots[slotIndex];

        if (slot.IsEmpty())
            return;

        DragItemManager.Instance.StartDrag(
            slot.item,
            slot.amount,
            DragSourceType.ActionBar,
            slotIndex
        );

        icon.color = new Color(1, 1, 1, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        icon.color = Color.white;

        if (DragItemManager.Instance.IsDragging)
            DragItemManager.Instance.ClearDrag();
    }

    IEnumerator Shake()
    {
        Vector3 original = icon.rectTransform.localPosition;

        float duration = 0.2f;
        float strength = 8f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float x = Mathf.Sin(timer * 40f) * strength;

            icon.rectTransform.localPosition =
                original + new Vector3(x, 0, 0);

            yield return null;
        }

        icon.rectTransform.localPosition = original;
    }
}