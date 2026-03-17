using UnityEngine;
using TMPro;
using System.Collections;

public class CameraIntroSequence : MonoBehaviour
{
    public Camera introCamera;
    public Camera playerCamera;

    [Header("Zoom")]
    public float startZoom = 12f;
    public float endZoom = 8.4f;
    public float zoomDuration = 2.5f;

    [Header("Text")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI modeText;

    void Start()
    {
        GameInputLock.Lock();
        StartCoroutine(IntroSequence());
    }


    IEnumerator IntroSequence()
    {
        // 🔒 bloquear gameplay
        GameIntroState.IsIntroPlaying = true;
        GamePause.Pause();

        // 🎥 activar cámara de intro
        playerCamera.enabled = false;
        introCamera.enabled = true;

        // reset textos
        titleText.alpha = 0;
        modeText.alpha = 0;

        // zoom inicial
        introCamera.orthographicSize = startZoom;

        float t = 0f;

        // 🔥 ZOOM CINEMÁTICO
        while (t < zoomDuration)
        {
            t += Time.unscaledDeltaTime;

            float progress = t / zoomDuration;

            // ease-out (más cinematográfico)
            progress = 1f - Mathf.Pow(1f - progress, 3f);

            introCamera.orthographicSize =
                Mathf.Lerp(startZoom, endZoom, progress);

            yield return null;
        }

        introCamera.orthographicSize = endZoom;

        // texto principal
        yield return FadeText(titleText, 1f, 0.6f);

        yield return new WaitForSecondsRealtime(0.3f);

        // modo
        yield return FadeText(modeText, 1f, 0.5f);

        yield return new WaitForSecondsRealtime(2f);

        // desaparecer
        yield return FadeText(titleText, 0f, 0.4f);
        yield return FadeText(modeText, 0f, 0.4f);

        // cambiar cámaras
        introCamera.enabled = false;
        playerCamera.enabled = true;

        // desbloquear juego
        GamePause.Resume();
        GameInputLock.Unlock();
        GameIntroState.IsIntroPlaying = false;
    }

    IEnumerator FadeText(TextMeshProUGUI text, float target, float duration)
    {
        float start = text.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            text.alpha = Mathf.Lerp(start, target, t / duration);

            yield return null;
        }

        text.alpha = target;
    }
}

public static class GameIntroState
{
    public static bool IsIntroPlaying = true;
}
