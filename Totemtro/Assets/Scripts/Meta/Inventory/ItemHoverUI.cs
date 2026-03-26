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
        ItemData item = GetItem();

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

        yield return new WaitForSecondsRealtime(0.5f); // ← Realtime, ignora timeScale

        if (ItemTooltipUI.Instance == null)
            yield break;

        RectTransform slotRect = GetSlotRect();
        ItemTooltipUI.Instance.Show(item, slotRect);
    }

    // ===============================
    // POINTER EXIT
    // ===============================

    public void OnPointerExit(PointerEventData eventData)
    {

        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }

        if (ItemTooltipUI.Instance != null)
            ItemTooltipUI.Instance.Hide();
    }
}
