using UnityEngine;

public class BagUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform parent;
    public int bagSize = 15;

    HubSlotUI[] slots;
    bool generated = false;

    void Awake()
    {
        // Generamos en Awake — corre aunque el panel esté desactivado
        Generate();
    }

    void Start()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged += RefreshAll;
        else
            StartCoroutine(WaitAndSubscribe());
    }

    System.Collections.IEnumerator WaitAndSubscribe()
    {
        while (MetaInventory.Instance == null)
            yield return null;

        MetaInventory.Instance.onInventoryChanged += RefreshAll;
        RefreshAll();
    }

    void OnEnable()
    {
        if (generated)
            RefreshAll();
    }

    void OnDestroy()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged -= RefreshAll;
    }

    void Generate()
    {
        slots = new HubSlotUI[bagSize];

        for (int i = 0; i < bagSize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, parent);
            HubSlotUI ui = slotObj.GetComponent<HubSlotUI>();

            ui.slotType = DragSource.Bag;
            ui.slotIndex = i;

            slots[i] = ui;
            slotObj.name = "BagSlot_" + i;
        }

        generated = true;
        RefreshAll();
    }

    void RefreshAll()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].Refresh();
        }
    }
}