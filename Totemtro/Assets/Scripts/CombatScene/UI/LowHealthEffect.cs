using UnityEngine;
using UnityEngine.UI;

public class LowHealthEffect : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image redOverlay;
    public RectTransform marco;

    [Header("Health Settings")]
    [Range(0f, 1f)]
    public float threshold = 0.2f;

    [Header("Pulse Settings")]
    public float basePulseSpeed = 2f;
    public float maxPulseSpeed = 4f;

    [Header("Overlay Alpha")]
    public float minOverlayAlpha = 0.01f;
    public float maxOverlayAlpha = 0.45f;

    [Header("Marco Alpha")]
    public float minMarcoAlpha = 0.3f;
    public float maxMarcoAlpha = 0.6f;

    [Header("Scale")]
    public float maxScale = 1.08f;

    [Header("Shake")]
    public float baseShakeIntensity = 0.03f;
    public float maxShakeIntensity = 0.08f;

    bool isActive = false;

    void Start()
    {
        SetOverlayAlpha(0f);
        SetMarcoAlpha(0f);

        if (marco != null)
            marco.localScale = Vector3.one;
    }

    void Update()
    {
        if (playerHealth == null)
            return;

        float hpPercent =
            playerHealth.GetCurrentHealthPercent();

        if (hpPercent <= threshold)
        {
            isActive = true;

            // 🔥 Cuanto menos vida, más intensidad
            float dangerFactor =
                1f - (hpPercent / threshold);

            float pulseSpeed =
                Mathf.Lerp(basePulseSpeed,
                           maxPulseSpeed,
                           dangerFactor);

            float pulse =
                Mathf.Sin(Time.time * pulseSpeed * Mathf.PI);

            pulse = Mathf.Clamp01(pulse);

            // 🔴 FONDO
            float overlayAlpha =
                Mathf.Lerp(minOverlayAlpha,
                           maxOverlayAlpha,
                           pulse);

            SetOverlayAlpha(overlayAlpha);

            // 💓 MARCO SCALE + ALPHA
            if (marco != null)
            {
                float scale =
                    Mathf.Lerp(1f, maxScale, pulse);

                marco.localScale =
                    Vector3.one * scale;

                float marcoAlpha =
                    Mathf.Lerp(minMarcoAlpha,
                               maxMarcoAlpha,
                               pulse);

                SetMarcoAlpha(marcoAlpha);
            }

            // 📳 SHAKE SOLO EN PICO
            if (pulse > 0.95f)
            {
                float shake =
                    Mathf.Lerp(baseShakeIntensity,
                               maxShakeIntensity,
                               dangerFactor);

                CameraShake.ShakeCamera(
                    shake,
                    0.08f
                );
            }
        }
        else
        {
            if (isActive)
            {
                isActive = false;

                SetOverlayAlpha(0f);
                SetMarcoAlpha(0f);

                if (marco != null)
                    marco.localScale = Vector3.one;
            }
        }
    }

    void SetOverlayAlpha(float a)
    {
        if (redOverlay == null)
            return;

        Color c = redOverlay.color;
        c.a = a;
        redOverlay.color = c;
    }

    void SetMarcoAlpha(float a)
    {
        if (marco == null)
            return;

        Image img = marco.GetComponent<Image>();

        if (img == null)
            return;

        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}