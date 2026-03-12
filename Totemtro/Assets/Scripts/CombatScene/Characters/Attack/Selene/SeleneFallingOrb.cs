using UnityEngine;

public class SeleneFallingOrb : MonoBehaviour
{
    [Header("Timing")]
    public float totalDuration = 0.8f;

    [Header("Scale Simulation")]
    public float maxHeightScale = 1.6f;
    public float minHeightScale = 0.6f;

    [Header("Floating Animation")]
    public float floatAmplitude = 0.08f;
    public float floatSpeed = 3f;

    public float pulseAmount = 0.08f;
    public float pulseSpeed = 4f;

    [Header("References")]
    public Transform shadow;
    public GameObject poolPrefab;
    public GameObject impactVFX;

    float timer;
    Vector3 startPos;
    bool hasImpacted = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (hasImpacted) return;

        timer += Time.deltaTime;

        float t = timer / totalDuration;

        // curva parabólica
        float height = Mathf.Sin(t * Mathf.PI);

        // escala base
        float scale = Mathf.Lerp(minHeightScale, maxHeightScale, height);

        // bombeo mágico
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        // flotación vertical suave
        float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.localScale = Vector3.one * scale * pulse;

        transform.position =
            new Vector3(startPos.x, startPos.y + floatOffset, startPos.z);

        // sombra se hace más grande al caer
        if (shadow != null)
        {
            float shadowScale = Mathf.Lerp(1.2f, 0.5f, height);
            shadow.localScale = Vector3.one * shadowScale;
        }

        if (t >= 1f)
        {
            Impact();
        }
    }

    void Impact()
    {
        if (hasImpacted) return;

        hasImpacted = true;

        if (poolPrefab != null)
            Instantiate(poolPrefab, startPos, Quaternion.identity);

        if (impactVFX != null)
            Instantiate(impactVFX, startPos, Quaternion.identity);

        Destroy(gameObject);
    }
}