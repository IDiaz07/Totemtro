using UnityEngine;

public class InventoryPanelUI : MonoBehaviour
{
    public RunInventory inventory;
    public GameObject slotPrefab;
    public Transform gridContainer;

    InventorySlotUI[] slotsUI;

    void Start()
    {
        GenerateSlots();

        if (inventory != null)
            inventory.onInventoryChanged += RefreshUI;

        RefreshUI();
    }

    void GenerateSlots()
    {
        slotsUI = new InventorySlotUI[inventory.maxSlots];

        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject obj =
                Instantiate(slotPrefab, gridContainer);

            InventorySlotUI ui =
                obj.GetComponent<InventorySlotUI>();

            ui.slotIndex = i;
            slotsUI[i] = ui;
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slotsUI.Length; i++)
        {
            InventorySlot slot = inventory.slots[i];
            slotsUI[i].Setup(slot.item, slot.amount);
        }
    }
}
