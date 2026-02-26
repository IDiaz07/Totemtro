using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public bool pauseGame = true;

    bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        if (!GameStateManager.Instance.CanOpenInventory())
            return;

        isOpen = !isOpen;

        inventoryPanel.SetActive(isOpen);

        if (pauseGame)
        {
            Time.timeScale = isOpen ? 0f : 1f;
        }

        GameStateManager.Instance.SetState(isOpen ? GameState.Inventory : GameState.Gameplay);
    }
}
