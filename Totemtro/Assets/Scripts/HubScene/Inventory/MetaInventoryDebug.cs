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

        MetaInventory.Instance.AddItem(testItem, amount);
        Debug.Log("Item añadido al MetaInventory");
    }
}