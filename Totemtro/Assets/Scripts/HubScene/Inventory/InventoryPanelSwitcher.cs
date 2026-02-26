using UnityEngine;

public class InventoryPanelSwitcher : MonoBehaviour
{
    public RectTransform armorPanel;
    public RectTransform loadoutPanel;

    public float slideSpeed = 10f;

    Vector2 armorVisiblePos;
    Vector2 armorHiddenPos;

    Vector2 loadoutVisiblePos;
    Vector2 loadoutHiddenPos;

    bool showingLoadout = false;

    void Start()
    {
        armorVisiblePos = armorPanel.anchoredPosition;
        loadoutVisiblePos = loadoutPanel.anchoredPosition;

        // 🔥 Ambos ocultos hacia la DERECHA
        armorHiddenPos = armorVisiblePos + new Vector2(armorPanel.rect.width, 0);
        loadoutHiddenPos = loadoutVisiblePos + new Vector2(loadoutPanel.rect.width, 0);

        // Loadout empieza oculto a la derecha
        loadoutPanel.anchoredPosition = loadoutHiddenPos;
    }

    void Update()
    {
        if (showingLoadout)
        {
            // Armor se va a la derecha
            armorPanel.anchoredPosition =
                Vector2.Lerp(armorPanel.anchoredPosition, armorHiddenPos, Time.deltaTime * slideSpeed);

            // Loadout entra al centro
            loadoutPanel.anchoredPosition =
                Vector2.Lerp(loadoutPanel.anchoredPosition, loadoutVisiblePos, Time.deltaTime * slideSpeed);
        }
        else
        {
            // Armor vuelve al centro
            armorPanel.anchoredPosition =
                Vector2.Lerp(armorPanel.anchoredPosition, armorVisiblePos, Time.deltaTime * slideSpeed);

            // Loadout se va a la derecha
            loadoutPanel.anchoredPosition =
                Vector2.Lerp(loadoutPanel.anchoredPosition, loadoutHiddenPos, Time.deltaTime * slideSpeed);
        }
    }

    public void TogglePanels()
    {
        showingLoadout = !showingLoadout;
    }
}