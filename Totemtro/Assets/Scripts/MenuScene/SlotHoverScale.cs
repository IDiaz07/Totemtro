using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotHoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Asigna el RectTransform del icono aquí")]
    public RectTransform icon;

    public float hoverScale = 1.15f;
    public float speed = 10f;

    Vector3 targetScale = Vector3.one;

    void Start()
    {
        // Si no está asignado en el Inspector, lo busca
        if (icon == null)
        {
            HubSlotUI hub = GetComponent<HubSlotUI>();
            if (hub != null && hub.icon != null)
                icon = hub.icon.rectTransform;
        }

        if (icon != null)
            icon.localScale = Vector3.one;

        targetScale = Vector3.one;
    }

    void Update()
    {
        if (icon == null) return;

        icon.localScale = Vector3.Lerp(
            icon.localScale,
            targetScale,
            Time.deltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
    }
}