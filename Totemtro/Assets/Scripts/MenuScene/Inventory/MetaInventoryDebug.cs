using UnityEngine;

public class MetaInventoryDebug : MonoBehaviour
{
    public ItemData testItem;
    public int amount = 5;

    [ContextMenu("Add Test Item")]
    public void AddTestItem()
    {
        if (MetaInventory.Instance == null)
        {
            Debug.LogError("MetaInventory no existe");
            return;
        }

        if (!MetaInventory.Instance.IsInitialized)
        {
            Debug.LogWarning("MetaInventory no está inicializado aún.");
            return;
        }

        bool added = MetaInventory.Instance.AddItem(testItem, amount);
        Debug.Log("Intentó añadir item al MetaInventory, resultado: " + added);
    }

    [ContextMenu("CLEAR INVENTORY")]
    public void ClearInventory()
    {
        if (MetaInventory.Instance == null)
        {
            Debug.LogError("MetaInventory no existe");
            return;
        }

        var meta = MetaInventory.Instance;

        // INVENTORY
        for (int i = 0; i < meta.slots.Length; i++)
        {
            meta.slots[i].Clear();
        }

        // BAG
        if (meta.bagSlots != null)
        {
            for (int i = 0; i < meta.bagSlots.Length; i++)
            {
                meta.bagSlots[i].Clear();
            }
        }

        // ARMOR
        if (meta.armorSlots != null)
        {
            for (int i = 0; i < meta.armorSlots.Length; i++)
            {
                meta.armorSlots[i].Clear();
            }
        }

        meta.NotifyInventoryChanged();
        meta.SaveMetaInventory();

        Debug.Log("🔥 INVENTARIO COMPLETAMENTE BORRADO");
    }
}