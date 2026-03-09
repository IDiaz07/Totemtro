using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    Vector3 originalPosition;
    Coroutine currentShake;

    void Awake()
    {
        // 🔥 Si ya existe uno, destruir duplicado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        originalPosition = transform.localPosition;
    }

    public static void ShakeCamera(float intensity, float duration)
    {
        if (Instance == null)
            return;

        Instance.InternalShake(intensity, duration);
    }

    void InternalShake(float intensity, float duration)
    {
        if (currentShake != null && intensity < 0.2f)
            return;

        if (currentShake != null)
            StopCoroutine(currentShake);

        currentShake = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            Vector2 offset = Random.insideUnitCircle * intensity;

            transform.localPosition =
                originalPosition +
                new Vector3(offset.x, offset.y, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        currentShake = null;
    }
}