using UnityEngine;
using System.Collections;

public static class UIAnimationUtility
{
    public static IEnumerator FadeScaleIn(Transform target, float duration = 0.25f)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = target.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        target.localScale = Vector3.one * 0.8f;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float n = t / duration;

            cg.alpha = Mathf.SmoothStep(0, 1, n);
            target.localScale = Vector3.Lerp(
                Vector3.one * 0.8f,
                Vector3.one,
                Mathf.SmoothStep(0, 1, n)
            );

            yield return null;
        }

        cg.alpha = 1f;
        target.localScale = Vector3.one;
    }
}