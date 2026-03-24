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
        if (InputKeyBindings.Instance == null) return;

        if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Map))
        {
            ToggleMap();
        }
        else if (isOpen && InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Pause))
        {
            ToggleMap(); // cerrar con ESC
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