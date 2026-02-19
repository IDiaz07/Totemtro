using UnityEngine;
using TMPro;
using System.Collections;

public class BossNameCinematic : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_Text text;

    public IEnumerator ShowName(string bossName)
    {
        text.text = bossName;

        float t = 0f;

        // Fade In
        while (t < 1f)
        {
            canvasGroup.alpha = t;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(2f);

        t = 0f;

        // Fade Out
        while (t < 1f)
        {
            canvasGroup.alpha = 1f - t;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
