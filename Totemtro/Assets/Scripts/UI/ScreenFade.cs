using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void FadeIn(float duration)
    {
        StartCoroutine(Fade(0f, 1f, duration));
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(Fade(1f, 0f, duration));
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;

        while (t < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cg.alpha = to;
    }
}
