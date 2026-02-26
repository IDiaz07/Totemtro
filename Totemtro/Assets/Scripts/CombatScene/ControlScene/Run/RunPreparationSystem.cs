using UnityEngine;

public class RunPreparationSystem : MonoBehaviour
{
    public static RunPreparationSystem Instance;

    void Awake()
    {
        Instance = this;
    }

    public bool PrepareRun(InventorySlot[] loadout)
    {
        // Verificar que haya suficientes items en MetaInventory
        foreach (var slot in loadout)
        {
            if (slot.IsEmpty()) continue;

            if (!slot.item.isConsumableInLoadout)
                continue;

            int metaAmount = MetaInventory.Instance.GetAmount(slot.item);

            if (metaAmount < slot.amount)
            {
                Debug.Log("Not enough items in MetaInventory");
                return false;
            }
        }

        // Descontar items
        foreach (var slot in loadout)
        {
            if (slot.IsEmpty()) continue;

            if (!slot.item.isConsumableInLoadout)
                continue;

            MetaInventory.Instance.RemoveItem(slot.item, slot.amount);
        }

        MetaInventory.Instance.SaveMetaInventory();
        return true;
    }
}