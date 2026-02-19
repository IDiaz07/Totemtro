using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;

    public MonoBehaviour playerMovement;
    public Weapon playerWeapon;

    bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (!isOpen)
            OpenInventory();
        else
            CloseInventory();
    }

    void OpenInventory()
    {
        isOpen = true;

        inventoryPanel.SetActive(true);

        // 🔒 Bloquear input
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerWeapon != null)
            playerWeapon.enabled = false;
        Time.timeScale = 0f;
    }

    void CloseInventory()
    {
        isOpen = false;

        inventoryPanel.SetActive(false);

        // 🔓 Restaurar input
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerWeapon != null)
            playerWeapon.enabled = true;
        Time.timeScale = 1f;
    }
}
