using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathRedFlash : MonoBehaviour
{
    public Image redImage;
    public float flashSpeed = 6f;

    void OnEnable()
    {
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float alpha = 0f;

        // Fade IN rápido
        while (alpha < 0.6f)
        {
            alpha += flashSpeed * Time.unscaledDeltaTime;
            redImage.color = new Color(0.6f, 0f, 0f, alpha);
            yield return null;
        }

        // Fade OUT más lento
        while (alpha > 0f)
        {
            alpha -= flashSpeed * 0.5f * Time.unscaledDeltaTime;
            redImage.color = new Color(0.6f, 0f, 0f, alpha);
            yield return null;
        }
    }
}