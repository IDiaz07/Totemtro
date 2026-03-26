using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fillImage;

    [Header("Behavior")]
    public float hideDelay = 1f;
    public float fadeSpeed = 8f;

    [Header("Colors")]
    public Color highHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    Transform target;
    Vector3 offset = new Vector3(0, 1.5f, 0);

    float timer;
    CanvasGroup canvasGroup;

    Camera cam;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        cam = Camera.main; // para billboard
    }

    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 📍 POSICIÓN
        transform.position = target.position + offset;

        // 🎥 BILLBOARD (siempre mira a cámara)
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }

        // ⏱ TIMER
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha,
                0f,
                Time.deltaTime * fadeSpeed
            );
        }
    }

    public void SetHealth(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);

        // 📊 Fill
        fillImage.fillAmount = percent;

        // 🎨 Color dinámico correcto
        fillImage.color = GetHealthColor(percent);

        // 👁 Mostrar
        canvasGroup.alpha = 1f;

        // ⏱ Reset timer
        timer = hideDelay;
    }

    Color GetHealthColor(float percent)
    {
        if (percent > 0.5f)
        {
            // Verde → Amarillo
            return Color.Lerp(midHealthColor, highHealthColor, (percent - 0.5f) * 2f);
        }
        else
        {
            // Amarillo → Rojo (CORREGIDO)
            return Color.Lerp(lowHealthColor, midHealthColor, percent * 2f);
        }
    }
}