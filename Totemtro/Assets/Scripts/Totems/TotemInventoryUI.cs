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

    [Header("References")]
    public TotemSelectionUI selectionUI;

    [Header("Confirm UI")]
    public TotemSellConfirmUI confirmUI;

    CanvasGroup canvasGroup;
    RectTransform rect;
    TotemInventory inventory;

    const int MAX_SLOTS = 6;

    bool openedFromSelection = false;

    void Awake()
    {
        inventory = FindFirstObjectByType<TotemInventory>();

        if (panel == null) return;

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        rect = panel.GetComponent<RectTransform>();

        panel.SetActive(false);
    }

    public void SellAll()
    {
        if (inventory == null || confirmUI == null)
            return;

        confirmUI.OpenSellAll(() =>
        {
            TotemSellSystem sellSystem = FindFirstObjectByType<TotemSellSystem>();
            if (sellSystem == null) return;

            List<TotemData> copy = new List<TotemData>(inventory.ownedTotems);

            foreach (var totem in copy)
            {
                sellSystem.SellTotem(totem);
            }

            Refresh();
        });
    }


    IEnumerator SellAllRoutine()
    {
        TotemSellSystem sellSystem = FindFirstObjectByType<TotemSellSystem>();

        List<TotemData> copy = new List<TotemData>(inventory.ownedTotems);

        foreach (var totem in copy)
        {
            sellSystem.SellTotem(totem);
            yield return new WaitForSecondsRealtime(0.05f); // pequeña animación
        }

        Refresh();
    }


    // =========================================
    // TOGGLE
    // =========================================

    public void TogglePanel()
    {
        if (!panel.activeSelf)
            OpenInventory();
        else
            CloseInventory();
    }

    void OpenInventory()
    {
        openedFromSelection = false;

        if (selectionUI != null && selectionUI.panel != null && selectionUI.panel.activeSelf)
        {
            openedFromSelection = true;
            selectionUI.panel.SetActive(false);
        }
        else
        {
            GamePause.Pause();
        }

        panel.SetActive(true);
        Refresh();
        StartCoroutine(FadeIn());
    }

    public void CloseInventory()
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

        if (openedFromSelection)
        {
            selectionUI.panel.SetActive(true);
            // ⚠ NO tocamos pausa aquí
        }
        else
        {
            GamePause.Resume();
        }
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

            // 🔥 Inyectar referencia del confirmUI
            ui.confirmUI = confirmUI;

            if (i < inventory.ownedTotems.Count)
                ui.Setup(inventory.ownedTotems[i]);
            else
                ui.Setup(null);
        }
    }

}
