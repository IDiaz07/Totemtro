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

        if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Pause))
        {
            // Si un sub-panel está abierto, volver al menú principal
            if (isPaused && IsSubPanelOpen())
            {
                CloseAllSubPanels();
                return;
            }

            // Si el inventario está abierto, cerrar inventario primero
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameState.Inventory)
            {
                return;
            }

            TogglePause();
        }
    }

    // =========================================
    // PAUSE TOGGLE
    // =========================================

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (gameMenuPanel != null)
            gameMenuPanel.SetActive(isPaused);

        if (isPaused)
        {
            CloseAllSubPanels();
            GamePause.Pause();
        }
        else
        {
            CloseAllSubPanels();
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