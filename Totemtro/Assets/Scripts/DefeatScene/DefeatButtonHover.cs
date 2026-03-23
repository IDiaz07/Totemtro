using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DefeatButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animSpeed = 0.12f;

    [Header("Glow (optional)")]
    [SerializeField] private CanvasGroup glowOverlay;

    private Vector3 originalScale;
    private Coroutine currentAnim;

    void Awake()
    {
        originalScale = transform.localScale;

        if (glowOverlay != null)
            glowOverlay.alpha = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateTo(originalScale * hoverScale);

        if (glowOverlay != null)
            StartCoroutine(FadeGlow(0.4f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(originalScale);

        if (glowOverlay != null)
            StartCoroutine(FadeGlow(0f));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateTo(originalScale * pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateTo(originalScale * hoverScale);
    }

    void AnimateTo(Vector3 target)
    {
        if (currentAnim != null)
            StopCoroutine(currentAnim);

        currentAnim = StartCoroutine(ScaleTo(target));
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        float t = 0f;
        Vector3 start = transform.localScale;

        while (t < animSpeed)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, t / animSpeed);
            transform.localScale = Vector3.Lerp(start, target, progress);
            yield return null;
        }

        transform.localScale = target;
    }

    IEnumerator FadeGlow(float target)
    {
        float t = 0f;
        float start = glowOverlay.alpha;

        while (t < animSpeed)
        {
            t += Time.deltaTime;
            glowOverlay.alpha = Mathf.Lerp(start, target, t / animSpeed);
            yield return null;
        }

        glowOverlay.alpha = target;
    }
}