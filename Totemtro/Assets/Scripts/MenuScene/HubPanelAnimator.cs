using UnityEngine;
using System.Collections;

public class HubPanelAnimator : MonoBehaviour
{
    public float duration = 0.25f;

    RectTransform rect;
    CanvasGroup canvasGroup;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void PlayOpen()
    {
        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        float time = 0f;

        rect.localScale = Vector3.one * 0.8f;
        canvasGroup.alpha = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            rect.localScale = Vector3.Lerp(
                Vector3.one * 0.8f,
                Vector3.one,
                EaseOutBack(t)
            );

            canvasGroup.alpha = t;

            yield return null;
        }

        rect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
}