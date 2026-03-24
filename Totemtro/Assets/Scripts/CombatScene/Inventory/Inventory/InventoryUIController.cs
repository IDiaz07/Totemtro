using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public bool pauseGame = true;

    bool isOpen = false;

    void Update()
    {
        if (InputKeyBindings.Instance == null) return;

        // Abrir/cerrar con tecla de inventario
        if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Inventory))
        {
            ToggleInventory();
        }
        // ESC cierra el inventario si está abierto
        else if (isOpen && InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Pause))
        {
            CloseInventory();
        }
    }

    void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    void OpenInventory()
    {
        if (!GameStateManager.Instance.CanOpenInventory())
            return;

        isOpen = true;
        inventoryPanel.SetActive(true);

        if (pauseGame)
            GamePause.Pause();

        GameStateManager.Instance.SetState(GameState.Inventory);
    }

    void CloseInventory()
    {
        isOpen = false;
        inventoryPanel.SetActive(false);

        if (pauseGame)
            GamePause.Resume();

        GameStateManager.Instance.SetState(GameState.Gameplay);
    }
}
