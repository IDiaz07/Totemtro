using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LeverAnimator : MonoBehaviour
{
    [Header("Referencias")]
    public RectTransform leverRect;
    public Button leverButton;

    [Header("Timing")]
    public float pullDuration = 0.2f;
    public float holdDuration = 0.1f;
    public float returnDuration = 0.35f;

    // Valores iniciales (reposo)
    const float rotYStart = 0f;
    const float heightStart = 600f;
    const float posYStart = -55f;

    // Valores finales (palanca tirada)
    const float rotYEnd = -40f;
    const float heightEnd = 800f;
    const float posYEnd = -65f;

    bool isAnimating = false;

    void Awake()
    {
        if (leverRect == null)
            leverRect = GetComponent<RectTransform>();

        ResetToIdle();
    }

    void ResetToIdle()
    {
        leverRect.localEulerAngles = new Vector3(0f, 0f, 0f);
        leverRect.sizeDelta = new Vector2(leverRect.sizeDelta.x, heightStart);
        leverRect.anchoredPosition = new Vector2(leverRect.anchoredPosition.x, posYStart);
    }

    public void PlayPull(System.Action onPullDown = null)
    {
        if (isAnimating) return;
        StartCoroutine(PullRoutine(onPullDown));
    }

    IEnumerator PullRoutine(System.Action onPullDown)
    {
        isAnimating = true;
        if (leverButton != null) leverButton.interactable = false;

        // --- BAJAR ---
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / pullDuration;
            float e = EaseInQuad(Mathf.Clamp01(t));

            leverRect.localEulerAngles = new Vector3(Mathf.Lerp(0f, 45f, e), 0f, 0f);
            leverRect.sizeDelta = new Vector2(leverRect.sizeDelta.x, Mathf.Lerp(heightStart, heightEnd, e));
            leverRect.anchoredPosition = new Vector2(leverRect.anchoredPosition.x, Mathf.Lerp(posYStart, posYEnd, e));

            yield return new WaitForEndOfFrame(); // ← clave
        }

        leverRect.localEulerAngles = new Vector3(45f, 0f, 0f);
        leverRect.sizeDelta = new Vector2(leverRect.sizeDelta.x, heightEnd);
        leverRect.anchoredPosition = new Vector2(leverRect.anchoredPosition.x, posYEnd);

        onPullDown?.Invoke();

        yield return new WaitForSecondsRealtime(holdDuration); // este ya funciona con timeScale=0

        // --- VOLVER ---
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / returnDuration;
            float e = EaseOutBack(Mathf.Clamp01(t));

            leverRect.localEulerAngles = new Vector3(Mathf.Lerp(45f, 0f, e), 0f, 0f);
            leverRect.sizeDelta = new Vector2(leverRect.sizeDelta.x, Mathf.Lerp(heightEnd, heightStart, e));
            leverRect.anchoredPosition = new Vector2(leverRect.anchoredPosition.x, Mathf.Lerp(posYEnd, posYStart, e));

            yield return new WaitForEndOfFrame(); // ← clave
        }

        ResetToIdle();
        if (leverButton != null) leverButton.interactable = true;
        isAnimating = false;
    }

    float EaseInQuad(float t) => t * t;

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}