using UnityEngine;

public class WorldMapController : MonoBehaviour
{
    [Header("References")]
    public GameObject worldMapUI;
    public WorldMapSystem worldMapSystem;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.M;
    public KeyCode centerKey = KeyCode.C;

    bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();
        }

        if (isOpen && Input.GetKeyDown(centerKey))
        {
            CenterOnPlayer();
        }
    }

    void ToggleMap()
    {
        isOpen = !isOpen;
        worldMapUI.SetActive(isOpen);

        if (isOpen)
        {
            GamePause.Pause();

            // Restaurar última posición del mapa (NO centrar en jugador)
            if (worldMapSystem != null)
                worldMapSystem.RestoreCameraPosition();
        }
        else
        {
            GamePause.Resume();
        }
    }

    /// <summary>
    /// Centra la cámara del mapa en el jugador
    /// </summary>
    public void CenterOnPlayer()
    {
        if (worldMapSystem != null)
            worldMapSystem.CenterOnPlayer();
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}