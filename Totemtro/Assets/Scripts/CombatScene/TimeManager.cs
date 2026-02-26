using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void DoSlowMotion(float slowScale, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowMotionRoutine(slowScale, duration));
    }

    IEnumerator SlowMotionRoutine(float slowScale, float duration)
    {
        Time.timeScale = slowScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}