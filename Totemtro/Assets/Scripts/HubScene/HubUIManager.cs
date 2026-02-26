using UnityEngine;

public class HubUIManager : MonoBehaviour
{
    public static HubUIManager Instance;

    public GameObject inventoryPanel;
    public GameObject confirmationPanel;
    public GameObject fadePanel;

    void Awake()
    {
        Instance = this;
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    public void OpenConfirmation()
    {
        confirmationPanel.SetActive(true);
    }

    public void CloseConfirmation()
    {
        confirmationPanel.SetActive(false);
    }

    public void ShowFade()
    {
        fadePanel.SetActive(true);
    }
}