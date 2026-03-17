using System.Collections;
using UnityEngine;

public class MetaInventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform contentParent;

    HubSlotUI[] slotsUI;

    IEnumerator Start()
    {
        while (MetaInventory.Instance == null)
            yield return null;

        if (MetaInventory.Instance.IsInitialized)
        {
            BuildGrid();
            MetaInventory.Instance.onInventoryChanged += RefreshUI;
            RefreshUI();
            yield break;
        }

        bool handled = false;
        MetaInventory.Instance.OnInitialized += () =>
        {
            if (handled) return;
            handled = true;
            BuildGrid();
            MetaInventory.Instance.onInventoryChanged += RefreshUI;
            RefreshUI();
        };

        while (!MetaInventory.Instance.IsInitialized)
            yield return null;
    }

    void BuildGrid()
    {
        Debug.Log($"MetaInventoryUI.BuildGrid called. slotPrefab={(slotPrefab==null?"NULL":"ok")} contentParent={(contentParent==null?"NULL":"ok")}");
        if (slotPrefab == null)
        {
            Debug.LogError("MetaInventoryUI: slotPrefab no está asignado en el inspector. No se pueden crear slots.");
            return;
        }
        if (contentParent == null)
        {
            Debug.LogError("MetaInventoryUI: contentParent no está asignado en el inspector.");
            return;
        }

        int count = MetaInventory.Instance.slots != null ? MetaInventory.Instance.slots.Length : 0;

        Debug.Log("BUILD GRID SLOTS: " + count);

        slotsUI = new HubSlotUI[count];

        for (int i = 0; i < count; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, contentParent);

            HubSlotUI ui = slotObj.GetComponent<HubSlotUI>();
            if (ui == null)
            {
                Debug.LogError("slotPrefab no contiene HubSlotUI. Revisar prefab.");
                continue;
            }

            ui.slotType = DragSource.Meta;
            ui.slotIndex = i;

            slotsUI[i] = ui;
        }
    }

    public void RefreshUI()
    {
        if (slotsUI == null) return;

        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (slotsUI[i] != null)
                slotsUI[i].Refresh();
        }
    }
}