using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class CardHoverProEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerMoveHandler
{
    [Header("Scale")]
    public float hoverScale = 1.1f;
    public float scaleSpeed = 12f;

    [Header("3D Tilt")]
    public float tiltAmount = 10f;
    public float tiltSpeed = 10f;

    [Header("Shadow")]
    public Shadow dynamicShadow;
    public float shadowDistance = 12f;

    [Header("Shine")]
    public Image shineImage;
    public float shineSpeed = 3f;

    RectTransform rect;
    Vector3 originalScale;
    Quaternion originalRot;

    bool hovering = false;
    Vector2 mouseLocalPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        originalRot = rect.localRotation;
    }

    void Update()
    {
        AnimateScale();
        AnimateTilt();
        AnimateShadow();
        AnimateShine();
    }

    void AnimateScale()
    {
        Vector3 target =
            hovering ? originalScale * hoverScale : originalScale;

        rect.localScale = Vector3.Lerp(
            rect.localScale,
            target,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    void AnimateTilt()
    {
        if (!hovering)
        {
            rect.localRotation = Quaternion.Lerp(
                rect.localRotation,
                originalRot,
                Time.unscaledDeltaTime * tiltSpeed
            );
            return;
        }

        float tiltX = -mouseLocalPos.y * tiltAmount;
        float tiltY = mouseLocalPos.x * tiltAmount;

        Quaternion targetRot =
            Quaternion.Euler(tiltX, tiltY, 0f);

        rect.localRotation = Quaternion.Lerp(
            rect.localRotation,
            targetRot,
            Time.unscaledDeltaTime * tiltSpeed
        );
    }

    void AnimateShadow()
    {
        if (dynamicShadow == null) return;

        Vector2 target =
            hovering ?
            new Vector2(shadowDistance, -shadowDistance)
            : Vector2.zero;

        dynamicShadow.effectDistance = Vector2.Lerp(
            dynamicShadow.effectDistance,
            target,
            Time.unscaledDeltaTime * 10f
        );
    }

    void AnimateShine()
    {
        if (!hovering || shineImage == null)
            return;

        shineImage.rectTransform.localPosition =
            new Vector3(
                Mathf.PingPong(
                    Time.unscaledTime * shineSpeed,
                    rect.rect.width
                ) - rect.rect.width / 2f,
                0f,
                0f
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        StartCoroutine(Punch());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out local
        );

        mouseLocalPos = new Vector2(
            local.x / rect.rect.width,
            local.y / rect.rect.height
        );
    }

    IEnumerator Punch()
    {
        float t = 0f;
        float duration = 0.15f;

        while (t < duration)
        {
            float scale =
                1f + Mathf.Sin(t * 40f) * 0.03f;

            rect.localScale =
                originalScale * hoverScale * scale;

            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
