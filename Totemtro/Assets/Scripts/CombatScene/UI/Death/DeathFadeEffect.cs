using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathFadeEffect : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 4f;

    void OnEnable()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float alpha = 0f;

        while (alpha < 0.75f)
        {
            alpha += fadeSpeed * Time.unscaledDeltaTime;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}