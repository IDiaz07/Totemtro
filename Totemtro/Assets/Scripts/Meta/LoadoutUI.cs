using UnityEngine;

public class LoadoutUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform parent;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        for (int i = 0; i < 15; i++)
        {
            GameObject slot = Instantiate(slotPrefab, parent);

            HubLoadoutSlotUI ui = slot.GetComponent<HubLoadoutSlotUI>();
            ui.slotIndex = i;
        }
    }
}