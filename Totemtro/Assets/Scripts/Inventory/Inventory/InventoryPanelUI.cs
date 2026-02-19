using UnityEngine;
using System.Collections.Generic;

public class InventoryPanelUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public GameObject slotPrefab;
    public Transform gridContainer;

    public int totalSlots = 15;

    InventorySlotUI[] slotsUI;

    void Awake()
    {
        GenerateSlots();
    }

    void Start()
    {
        playerInventory.onInventoryChanged += RefreshUI;
        RefreshUI();
    }

    void OnEnable()
    {
        RefreshUI();
    }

    void GenerateSlots()
    {
        slotsUI = new InventorySlotUI[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj =
                Instantiate(slotPrefab, gridContainer);

            InventorySlotUI slotUI =
                slotObj.GetComponent<InventorySlotUI>();

            slotsUI[i] = slotUI;
        }
    }

    public void RefreshUI()
    {
        if (playerInventory == null)
            return;

        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (i < playerInventory.items.Count)
            {
                var slot = playerInventory.items[i];
                slotsUI[i].Setup(slot.item, slot.amount);
            }
            else
            {
                // 👇 Limpia slot vacío
                slotsUI[i].Setup(null, 0);
            }
        }
    }

}
