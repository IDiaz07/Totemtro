using UnityEngine;

public class MetaInventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform contentParent;

    void Start()
    {
        if (MetaInventory.Instance == null)
        {
            Debug.LogError("MetaInventory Instance es NULL");
            return;
        }

        RefreshUI();
        MetaInventory.Instance.onInventoryChanged += RefreshUI;
    }

    void OnDestroy()
    {
        if (MetaInventory.Instance != null)
            MetaInventory.Instance.onInventoryChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        if (MetaInventory.Instance == null)
            return;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        int count = MetaInventory.Instance.slots.Length;

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, contentParent);
            slot.GetComponent<HubMetaSlotUI>().slotIndex = i;
        }
    }
}