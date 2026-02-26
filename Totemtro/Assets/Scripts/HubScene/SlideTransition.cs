using UnityEngine;
using System.Collections;

public class SlideTransition : MonoBehaviour
{
    public RectTransform inventoryPanel;
    public RectTransform loadoutPanel;

    public float duration = 0.25f;

    public void ShowLoadout()
    {
        StopAllCoroutines();
        StartCoroutine(Slide(
            inventoryPanel.anchoredPosition,
            new Vector2(-1200, 0),
            loadoutPanel.anchoredPosition,
            Vector2.zero
        ));
    }

    public void ShowInventory()
    {
        StopAllCoroutines();
        StartCoroutine(Slide(
            inventoryPanel.anchoredPosition,
            Vector2.zero,
            loadoutPanel.anchoredPosition,
            new Vector2(1200, 0)
        ));
    }

    IEnumerator Slide(
        Vector2 invStart,
        Vector2 invEnd,
        Vector2 loadStart,
        Vector2 loadEnd)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            inventoryPanel.anchoredPosition =
                Vector2.Lerp(invStart, invEnd, t);

            loadoutPanel.anchoredPosition =
                Vector2.Lerp(loadStart, loadEnd, t);

            yield return null;
        }
    }
}