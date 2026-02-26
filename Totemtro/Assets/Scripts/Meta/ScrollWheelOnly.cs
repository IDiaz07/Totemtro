using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class ScrollWheelOnly : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IInitializePotentialDragHandler
{
    ScrollRect scrollRect;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    // Evita que el ScrollRect empiece a arrastrar
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    // Cancela el drag al empezar
    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = null;
    }

    // Bloquea movimiento por drag
    public void OnDrag(PointerEventData eventData)
    {
        // No hacemos nada → bloquea el drag
    }
}