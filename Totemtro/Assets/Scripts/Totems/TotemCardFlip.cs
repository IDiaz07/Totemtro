using UnityEngine;
using System.Collections;

public class TotemCardFlip : MonoBehaviour
{
    public GameObject front;
    public GameObject back;

    public float flipDuration = 0.25f;

    bool isFlipped = false;
    bool isAnimating = false;

    void Start()
    {
        front.SetActive(true);
        back.SetActive(false);
    }

    public void Flip()
    {
        if (isAnimating) return;
        StartCoroutine(FlipRoutine());
    }

    IEnumerator FlipRoutine()
    {
        isAnimating = true;

        float halfDuration = flipDuration * 0.5f;
        float t = 0f;

        // Primera mitad (0 → 90)
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / halfDuration;
            float angle = Mathf.Lerp(0f, 90f, t);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }

        // Cambio de cara
        front.SetActive(isFlipped);
        back.SetActive(!isFlipped);

        // 🔥 RESET ROTACIÓN
        transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        t = 0f;

        // Segunda mitad (-90 → 0)
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / halfDuration;
            float angle = Mathf.Lerp(-90f, 0f, t);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }

        transform.localRotation = Quaternion.identity;

        isFlipped = !isFlipped;
        isAnimating = false;
    }
}
