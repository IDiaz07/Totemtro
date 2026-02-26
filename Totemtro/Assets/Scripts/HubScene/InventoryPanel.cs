using UnityEngine;

public class InventoryTabController : MonoBehaviour
{
    public GameObject metaTab;
    public GameObject loadoutTab;

    public void ShowMeta()
    {
        metaTab.SetActive(true);
        loadoutTab.SetActive(false);
    }

    public void ShowLoadout()
    {
        metaTab.SetActive(false);
        loadoutTab.SetActive(true);
    }
}