using UnityEngine;

public class MetaInventoryDiagnostics : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(LogStatus), 0.5f);
    }

    void LogStatus()
    {
        Debug.Log("=== MetaInventory Diagnostics ===");

        Debug.Log("SaveSystem.Instance: " + (SaveSystem.Instance != null));
        Debug.Log("SaveSystem.IsReady: " + (SaveSystem.Instance != null ? SaveSystem.Instance.IsReady.ToString() : "n/a"));

        Debug.Log("ItemDatabase.Instance: " + (ItemDatabase.Instance != null));
        Debug.Log("MetaInventory.Instance: " + (MetaInventory.Instance != null));

        if (MetaInventory.Instance != null)
        {
            Debug.Log("MetaInventory.IsInitialized: " + MetaInventory.Instance.IsInitialized);
            Debug.Log("MetaInventory.slots: " + (MetaInventory.Instance.slots != null ? MetaInventory.Instance.slots.Length.ToString() : "null"));
            Debug.Log("MetaInventory.bagSlots: " + (MetaInventory.Instance.bagSlots != null ? MetaInventory.Instance.bagSlots.Length.ToString() : "null"));
        }

        if (EquipmentSystem.Instance != null)
        {
            Debug.Log("EquipmentSystem OK");

            var eq = EquipmentSystem.Instance.equipmentSlots;

            for (int i = 0; i < eq.Length; i++)
            {
                var slot = eq[i];

                if (slot != null && slot.item != null)
                    Debug.Log($"Armor Slot {i}: {slot.item.itemID} ({slot.durability})");
                else
                    Debug.Log($"Armor Slot {i}: EMPTY");
            }
        }
    }
}