using UnityEngine;
using System.Collections;

public class BossPortal : MonoBehaviour
{
    public float openDuration = 1.2f;
    public float maxScale = 2.5f;
    public float pulseSpeed = 4f;
    public GameObject rayPrefab;
    public int rayCount = 8;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(OpenPortal());
    }

    IEnumerator OpenPortal()
    {
        float t = 0f;

        // 📈 ESCALADO INICIAL
        while (t < openDuration)
        {
            float curve = Mathf.SmoothStep(0, 1, t / openDuration);

            transform.localScale =
                Vector3.one * Mathf.Lerp(0f, maxScale, curve);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one * maxScale;

        // ⚡ Generar rayos
        SpawnRays();

        // 🌫 Comienza pulsación
        StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        while (true)
        {
            float scale =
                maxScale +
                Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.1f;

            transform.localScale =
                Vector3.one * scale;

            yield return null;
        }
    }

    void SpawnRays()
    {
        if (rayPrefab == null) return;

        for (int i = 0; i < rayCount; i++)
        {
            float angle =
                (360f / rayCount) * i;

            GameObject ray =
                Instantiate(rayPrefab, transform.position, Quaternion.identity);

            ray.transform.rotation =
                Quaternion.Euler(0, 0, angle);

            Destroy(ray, 1.2f);
        }
    }
}
