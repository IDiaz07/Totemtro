using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour,
    IDropHandler,
    IPointerClickHandler
{
    public int slotIndex;
    public Image icon;

    void OnEnable()
    {
        if (EquipmentSystem.Instance == null)
            return;

        EquipmentSystem.Instance.onEquipmentChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (EquipmentSystem.Instance == null)
            return;

        EquipmentSystem.Instance.onEquipmentChanged -= Refresh;
    }

    void Refresh()
    {
        if (EquipmentSystem.Instance == null)
            return;

        if (slotIndex < 0 || slotIndex >= EquipmentSystem.Instance.equipmentSlots.Length)
            return;

        var slot = EquipmentSystem.Instance.equipmentSlots[slotIndex];

        if (slot.item == null)
        {
            icon.enabled = false;
            return;
        }

        icon.enabled = true;
        icon.sprite = slot.item.icon;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var drag = MetaDragUI.Instance;
        var equipment = EquipmentSystem.Instance;

        if (equipment == null || drag == null)
            return;

        if (!drag.IsDragging || drag.draggedItem == null)
            return;

        if (drag.draggedItem.itemType != ItemType.Equipment)
            return;

        equipment.EquipItem(drag.draggedItem);

        drag.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EquipmentSystem.Instance == null)
            return;

        EquipmentSystem.Instance.Unequip(slotIndex);
    }
}