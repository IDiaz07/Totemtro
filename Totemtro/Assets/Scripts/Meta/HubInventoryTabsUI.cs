using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HubInventoryTabsUI : MonoBehaviour
{
    [Header("Panel Switcher")]
    public InventoryPanelSwitcher panelSwitcher;

    [Header("UI")]
    public TMP_Text TextAB;
    public Image image;
    public Sprite armor;
    public Sprite bag;

    bool isLoadoutActive = false;

    void Start()
    {
        SetInventoryVisual();
    }

    public void ToggleLoadout()
    {
        panelSwitcher.TogglePanels();

        isLoadoutActive = !isLoadoutActive;

        if (isLoadoutActive)
            SetLoadoutVisual();
        else
            SetInventoryVisual();
    }

    void SetInventoryVisual()
    {
        TextAB.text = "ARMOR";
        image.sprite = bag;
    }

    void SetLoadoutVisual()
    {
        TextAB.text = "BAG";
        image.sprite = armor;
    }
}