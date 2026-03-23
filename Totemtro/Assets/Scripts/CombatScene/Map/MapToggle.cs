using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MapToggle : MonoBehaviour
{
    public GameObject worldMapUI;
    public GameObject minimapUI;

    [Header("Lights")]
    public Light2D playerLight;
    public Light2D mapGlobalLight;

    bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
    }

    void ToggleMap()
    {
        isOpen = !isOpen;

        worldMapUI.SetActive(isOpen);
        minimapUI.SetActive(!isOpen);

        if (isOpen)
        {
            GamePause.Pause();

            // 🔥 CAMBIO DE LUCES
            if (playerLight != null)
                playerLight.enabled = false;

            if (mapGlobalLight != null)
                mapGlobalLight.enabled = true;
        }
        else
        {
            GamePause.Resume();

            if (playerLight != null)
                playerLight.enabled = true;

            if (mapGlobalLight != null)
                mapGlobalLight.enabled = false;
        }
    }
}