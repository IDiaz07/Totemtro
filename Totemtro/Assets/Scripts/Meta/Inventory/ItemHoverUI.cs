using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemHoverTooltip : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    Coroutine hoverRoutine;

    // ===============================
    // GET ITEM
    // ===============================

    ItemData GetItem()
    {
        HubSlotUI hub = GetComponentInParent<HubSlotUI>();
        if (hub != null)
            return hub.GetCurrentItem();

        ActionSlotUI action = GetComponentInParent<ActionSlotUI>();
        if (action != null)
            return action.GetCurrentItem();

        return null;
    }

    // ===============================
    // GET SLOT RECT
    // ===============================

    RectTransform GetSlotRect()
    {
        HubSlotUI hub = GetComponentInParent<HubSlotUI>();
        if (hub != null)
            return hub.icon.rectTransform;

        ActionSlotUI action = GetComponentInParent<ActionSlotUI>();
        if (action != null)
            return action.icon.rectTransform;

        return GetComponent<RectTransform>();
    }

    // ===============================
    // POINTER ENTER
    // ===============================

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"ENTER: {gameObject.name} | pointerPos: {eventData.position} | presser: {eventData.pointerPress}");
        ItemData item = GetItem();
        Debug.Log("ITEM: " + (item != null ? item.itemName : "NULL"));

        if (item == null) return;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = StartCoroutine(HoverDelay(item));
    }

    // ===============================
    // HOVER DELAY
    // ===============================

    IEnumerator HoverDelay(ItemData item)
    {
        Debug.Log("[Tooltip] Corrutina iniciada para: " + item.itemName);

        yield return new WaitForSecondsRealtime(0.5f); // ← Realtime, ignora timeScale

        Debug.Log("[Tooltip] Despues del delay - Instance: " + (ItemTooltipUI.Instance != null ? "OK" : "NULL"));

        if (ItemTooltipUI.Instance == null)
            yield break;

        RectTransform slotRect = GetSlotRect();
        ItemTooltipUI.Instance.Show(item, slotRect);
        Debug.Log("[Tooltip] Show() llamado");
    }

    // ===============================
    // POINTER EXIT
    // ===============================

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[Tooltip] OnPointerExit: " + gameObject.name);

        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }

        if (ItemTooltipUI.Instance != null)
            ItemTooltipUI.Instance.Hide();
    }
}
