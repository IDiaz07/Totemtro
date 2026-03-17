using UnityEngine;
using System.Collections;

public class UIShake : MonoBehaviour
{
    public float duration = 0.35f;
    public float magnitude = 15f;

    Vector3 originalPos;

    public void Play()
    {
        StopAllCoroutines();
        originalPos = transform.localPosition;
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition =
                originalPos + new Vector3(x, y, 0);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}