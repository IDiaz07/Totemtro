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

    void Start()
    {
        // Empieza activado para que Awake/Start de los hijos corran,
        // luego lo cerramos por código
        CloseAll();
    }

    void Update()
    {
        if (GameInputLock.IsLocked)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            ToggleInventory();
    }

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

    void PauseGame()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerWeapon != null)
        {
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