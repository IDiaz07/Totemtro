using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ActionSlotUI : MonoBehaviour, IDropHandler
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
        if (InventorySlotUI.draggingFromIndex == -1)
            return;

        actionBar.AssignToSlot(
            InventorySlotUI.draggingFromIndex,
            slotIndex
        );

        InventorySlotUI.draggingFromIndex = -1;
    }
}