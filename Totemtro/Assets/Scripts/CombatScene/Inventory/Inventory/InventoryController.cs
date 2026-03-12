using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject totemPanel;
    public GameObject craftingPanel;

    [Header("Player")]
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

    // ===============================
    // TOGGLE GENERAL
    // ===============================

    public void ToggleInventory()
    {
        if (!isOpen)
            OpenInventory();
        else
            CloseAll();
    }

    void OpenInventory()
    {
        isOpen = true;

        PauseGame();

        ShowInventory();
    }

    public void CloseAll()
    {
        isOpen = false;

        inventoryPanel.SetActive(false);
        totemPanel.SetActive(false);
        craftingPanel.SetActive(false);

        ResumeGame();
    }

    // ===============================
    // PANEL SWITCHING
    // ===============================

    public void ShowInventory()
    {
        inventoryPanel.SetActive(true);
        totemPanel.SetActive(false);
        craftingPanel.SetActive(false);
    }

    public void ShowTotems()
    {
        inventoryPanel.SetActive(false);
        totemPanel.SetActive(true);
        craftingPanel.SetActive(false);
    }

    public void ShowCrafting()
    {
        inventoryPanel.SetActive(false);
        totemPanel.SetActive(false);
        craftingPanel.SetActive(true);
    }

    // ===============================
    // PAUSE CONTROL
    // ===============================

    void PauseGame()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerWeapon != null)
        {
            // cancelar ataques en curso
            playerWeapon.isAiming = false;
            playerWeapon.isAttacking = false;

            playerWeapon.enabled = false;
        }

        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerWeapon != null)
            playerWeapon.enabled = true;

        Time.timeScale = 1f;
    }
}
