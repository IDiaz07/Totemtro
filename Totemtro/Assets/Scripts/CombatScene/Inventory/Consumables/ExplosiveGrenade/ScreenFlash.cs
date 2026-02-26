using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;

    [SerializeField] Image flashImage;

    void Awake()
    {
        Instance = this;

        if (flashImage == null)
            flashImage = GetComponent<Image>();

        // 🔥 Solo invisible, pero el objeto sigue activo
        flashImage.color = new Color(1, 1, 1, 0);
    }

    public void Flash()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1, 1, 1, 0.8f);
        yield return new WaitForSecondsRealtime(0.05f);

        float t = 0f;
        float duration = 0.2f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(0.8f, 0f, t / duration);
            flashImage.color = new Color(1, 1, 1, alpha);

            yield return null;
        }

        flashImage.color = new Color(1, 1, 1, 0);
    }
}