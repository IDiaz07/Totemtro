using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TotemCardAnimator : MonoBehaviour
{
    CanvasGroup canvasGroup;

    Vector3 originalScale;
    Vector3 originalPos;
    Quaternion originalRot;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // Si por alguna razón no existe, lo añadimos
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    public void PlayAppear(int index, int total)
    {
        StopAllCoroutines();
        StartCoroutine(AppearRoutine(index));
    }

    IEnumerator AppearRoutine(int index)
    {
        // 🔥 Reset limpio siempre
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = originalPos + Vector3.down * 40f;

        // Delay escalonado entre cartas
        yield return new WaitForSecondsRealtime(index * 0.08f);

        float duration = 0.35f;
        float t = 0f;

        while (t < duration)
        {
            float smooth = t / duration;
            smooth = smooth * smooth * (3f - 2f * smooth); // SmoothStep

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, smooth);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, smooth);
            transform.localPosition = Vector3.Lerp(
                originalPos + Vector3.down * 40f,
                originalPos,
                smooth
            );

            t += Time.unscaledDeltaTime; // 🔥 CLAVE
            yield return null;
        }

        // Estado final asegurado
        canvasGroup.alpha = 1f;
        transform.localScale = originalScale;
        transform.localPosition = originalPos;
        transform.localRotation = originalRot;
    }
}
