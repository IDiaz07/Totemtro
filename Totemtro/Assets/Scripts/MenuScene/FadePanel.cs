using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadePanel : MonoBehaviour
{
    public Image image;

    void OnEnable()
    {
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}