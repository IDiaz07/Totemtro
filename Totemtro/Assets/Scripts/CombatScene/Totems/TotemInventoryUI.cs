using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TotemInventoryUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Transform container;
    public GameObject slotPrefab;

    [Header("Confirm UI")]
    public TotemSellConfirmUI confirmUI;

    [Header("Optional References")]
    public TotemSelectionUI selectionUI; // SOLO si viene de level up
    public GameObject inventoryPanel;    // SOLO si viene del menu
    public GameObject craftingPanel;     // SOLO si viene del menu

    [Header("Behavior")]
    public bool openedFromLevelUp = false; // 🔥 ESTA ES LA CLAVE

    CanvasGroup canvasGroup;
    RectTransform rect;
    TotemInventory inventory;

    const int MAX_SLOTS = 6;

    void Awake()
    {
        inventory = FindFirstObjectByType<TotemInventory>();

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        rect = panel.GetComponent<RectTransform>();
        panel.SetActive(false);
    }

    // =========================================
    // OPEN
    // =========================================

    public void Open()
    {
        // 🔒 Viene de level up
        GamePause.Pause();
        if (selectionUI != null)
           selectionUI.panel.SetActive(false);


        panel.SetActive(true);
        Refresh();
        StartCoroutine(FadeIn());
    }

    public void Close()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one * 0.9f;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 8f;
            canvasGroup.alpha = t;
            rect.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one;
        //GamePause.Reset();
    }

    IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 8f;
            canvasGroup.alpha = 1f - t;
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, t);
            yield return null;
        }

        panel.SetActive(false);
        selectionUI.panel.SetActive(true);
    }

    // =========================================
    // REFRESH
    // =========================================

    void Refresh()
    {
        if (inventory == null || container == null)
            return;

        foreach (Transform child in container)
            Destroy(child.gameObject);

        for (int i = 0; i < MAX_SLOTS; i++)
        {
            GameObject slot = Instantiate(slotPrefab, container);
            TotemInventorySlotUI ui = slot.GetComponent<TotemInventorySlotUI>();

            ui.confirmUI = confirmUI;

            if (i < inventory.ownedTotems.Count)
                ui.Setup(inventory.ownedTotems[i]);
            else
                ui.Setup(null);
        }
    }
}
