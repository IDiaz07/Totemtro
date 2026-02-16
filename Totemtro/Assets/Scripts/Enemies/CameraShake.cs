using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    Vector3 originalPosition;

    void Awake()
    {
        Instance = this;
        originalPosition = transform.localPosition;
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            Vector2 offset = Random.insideUnitCircle * intensity;

            transform.localPosition = originalPosition +
                new Vector3(offset.x, offset.y, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
