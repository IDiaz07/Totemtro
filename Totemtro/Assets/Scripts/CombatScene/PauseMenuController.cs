using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("Main Panel")]
    public GameObject gameMenuPanel;

    [Header("Sub Panels")]
    public GameObject optionsPanel;
    public GameObject statisticsPanel;

    bool isPaused = false;

    void Awake()
    {
        Instance = this;

        if (gameMenuPanel != null)
            gameMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);
    }

    void Update()
    {
        if (InputKeyBindings.Instance == null) return;

        if (!InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Pause)) return;

        // Si la SlotMachine está abierta → no hacer nada (ella gestiona su propio cierre)
        if (UILayerManager.IsOpen(UILayerManager.Layer.SlotMachine))
            return;

        // Si el inventario está abierto → no hacer nada (InventoryController lo cierra)
        if (UILayerManager.IsOpen(UILayerManager.Layer.Inventory))
            return;

        // Si hay un subpanel abierto dentro del pause menu → cerrar subpanel
        if (isPaused && IsSubPanelOpen())
        {
            CloseAllSubPanels();
            return;
        }

        // Si no hay nada abierto → toggle pause menu
        TogglePause();
    }

    // =========================================
    // PAUSE TOGGLE
    // =========================================

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (gameMenuPanel != null) gameMenuPanel.SetActive(isPaused);

        if (isPaused)
        {
            CloseAllSubPanels();
            UILayerManager.Open(UILayerManager.Layer.PauseMenu);
            GamePause.Pause();
        }
        else
        {
            CloseAllSubPanels();
            UILayerManager.Close(UILayerManager.Layer.PauseMenu);
            GamePause.Resume();
        }
    }

    // =========================================
    // BUTTON CALLBACKS
    // =========================================

    public void OnBackToGame()
    {
        if (!isPaused) return;
        TogglePause();
    }

    public void OnStatistics()
    {
        CloseAllSubPanels();

        if (statisticsPanel != null)
            statisticsPanel.SetActive(true);
    }

    public void OnOptions()
    {
        CloseAllSubPanels();

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void OnSaveAndQuit()
    {
        GamePause.Reset();

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToHub();
    }

    // =========================================
    // SUB PANELS
    // =========================================

    bool IsSubPanelOpen()
    {
        if (optionsPanel != null && optionsPanel.activeSelf)
            return true;

        if (statisticsPanel != null && statisticsPanel.activeSelf)
            return true;

        return false;
    }

    void CloseAllSubPanels()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);
    }
}