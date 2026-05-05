using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SlotHoverLiftGlow : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("References")]
    public RectTransform slotRoot;
    public RectTransform icon;
    public RectTransform glowIcon;
    public Image iconShadow;

    [Header("Lift Settings")]
    public float liftAmount = 6f;
    public float duration = 0.14f;

    [Header("Scale Settings")]
    public float iconScale = 1.1f;
    public float glowScale = 1.2f;

    [Header("Glow Settings")]
    public float glowAlpha = 0.7f;

    readonly Vector3 baseScale = Vector3.one;

    Vector2 originalIconPos;
    Vector3 originalGlowScale;
    Color originalShadowColor;

    float targetValue = 0f;
    float currentValue = 0f;

    bool ready = false;

    Image glowImage;

    void Awake()
    {
        if (slotRoot == null)
            slotRoot = GetComponent<RectTransform>();

        if (glowIcon != null)
        {
            originalGlowScale = glowIcon.localScale;
            glowImage = glowIcon.GetComponent<Image>();
        }

        if (iconShadow != null)
            originalShadowColor = iconShadow.color;

        SetGlow(1f);
    }

    void Initialize()
    {
        if (ready) return;

        if (icon != null)
            originalIconPos = icon.anchoredPosition;

        ready = true;
    }

    void Update()
    {
        if (Mathf.Approximately(currentValue, targetValue))
            return;

        currentValue = Mathf.MoveTowards(
            currentValue,
            targetValue,
            Time.deltaTime / duration
        );

        float eased = EaseOutCubic(currentValue);
        ApplyVisuals(eased);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Initialize();

        targetValue = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetValue = 0f;
    }

    void ApplyVisuals(float t)
    {

        // LIFT — mueve el icono, no el slotRoot
        if (icon != null)
        {
            icon.anchoredPosition =
                originalIconPos + Vector2.up * (liftAmount * t);
        }

        // ICON SCALE
        if (icon != null)
        {
            icon.localScale = Vector3.Lerp(
                baseScale,
                baseScale * iconScale,
                t
            );
        }

        // GLOW SCALE
        if (glowIcon != null)
        {
            glowIcon.localScale = Vector3.Lerp(
                originalGlowScale,
                originalGlowScale * glowScale,
                t
            );
        }

        // GLOW ALPHA — siempre 1
        if (glowImage != null)
        {
            Color c = glowImage.color;
            c.a = 1f;
            glowImage.color = c;
        }

        // SHADOW BOOST
        if (iconShadow != null)
        {
            Color shadowColor = originalShadowColor;
            shadowColor.a = Mathf.Lerp(
                originalShadowColor.a,
                originalShadowColor.a * 1.4f,
                t
            );
            iconShadow.color = shadowColor;
        }
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void SyncVisuals(Sprite newSprite, bool hasItem)
    {
        if (iconShadow != null)
        {
            iconShadow.enabled = hasItem;
            if (hasItem)
                iconShadow.sprite = newSprite;
        }

        if (glowImage != null)
        {
            glowImage.enabled = hasItem;
            if (hasItem)
                glowImage.sprite = newSprite;
        }
    }

    void SetGlow(float alpha)
    {
        if (glowImage == null) return;
        Color c = glowImage.color;
        c.a = alpha;
        glowImage.color = c;
    }
}