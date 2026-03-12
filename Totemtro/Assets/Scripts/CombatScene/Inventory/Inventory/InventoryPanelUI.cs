using UnityEngine;

public class InventoryPanelUI : MonoBehaviour
{
    [Header("References")]
    public RunInventory inventory;
    public GameObject slotPrefab;
    public Transform gridContainer;

    InventorySlotUI[] slotsUI;

    void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<RunInventory>();
    }

    void Start()
    {
        if (inventory == null)
        {
            Debug.LogError("InventoryPanelUI: RunInventory not assigned.");
            return;
        }

        GenerateSlots();

        inventory.onInventoryChanged += RefreshUI;

        RefreshUI();
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.onInventoryChanged -= RefreshUI;
    }

    void GenerateSlots()
    {
        if (slotPrefab == null || gridContainer == null)
        {
            Debug.LogError("InventoryPanelUI: slotPrefab or gridContainer missing.");
            return;
        }

        // limpiar slots antiguos si existen
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        slotsUI = new InventorySlotUI[inventory.maxSlots];

        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, gridContainer);

            InventorySlotUI ui = obj.GetComponent<InventorySlotUI>();

            if (ui == null)
            {
                Debug.LogError("Slot prefab missing InventorySlotUI.");
                continue;
            }

            ui.slotIndex = i;

            slotsUI[i] = ui;
        }
    }

    public void RefreshUI()
    {
        if (inventory == null || slotsUI == null)
            return;

        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (slotsUI[i] == null)
                continue;

            InventorySlot slot = inventory.slots[i];

            slotsUI[i].Setup(slot.item, slot.amount);
        }
    }
}