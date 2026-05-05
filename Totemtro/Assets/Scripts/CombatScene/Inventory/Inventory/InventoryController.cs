using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject totemPanel;
    public GameObject craftingPanel;
    public GameObject chestPanel;
    public GameObject slotMachinePanel;

    [Header("Player")]
    public MonoBehaviour playerMovement;
    public Weapon playerWeapon;

    bool isOpen = false;
    public static InventoryController Instance;

    void Start()
    {
        // Empieza activado para que Awake/Start de los hijos corran,
        // luego lo cerramos por código
        CloseAll();
    }

    public bool IsInventoryOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    void Awake()
    {
        Instance = this;
    }

    void OpenInventory()
    {
        isOpen = true;
        UILayerManager.Open(UILayerManager.Layer.Inventory);
        PauseGame();
        ShowInventory();
    }

    public void CloseAll()
    {
        isOpen = false;
        UILayerManager.Close(UILayerManager.Layer.Inventory);
        inventoryPanel.SetActive(false);
        totemPanel.SetActive(false);
        craftingPanel.SetActive(false);
        ResumeGame();
    }

    void Update()
    {
        if (GameInputLock.IsLocked) return;

        if (InputKeyBindings.Instance != null)
        {
            if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Inventory))
                ToggleInventory();

            // Escape cierra el inventario si está abierto
            if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Pause)
                && UILayerManager.IsOpen(UILayerManager.Layer.Inventory))
            {
                CloseAll();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E)) ToggleInventory();
            if (Input.GetKeyDown(KeyCode.Escape)
                && UILayerManager.IsOpen(UILayerManager.Layer.Inventory))
                CloseAll();
        }
    }

    public void ToggleInventory()
    {
        if (!isOpen)
            OpenInventory();
        else
            CloseAll();
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