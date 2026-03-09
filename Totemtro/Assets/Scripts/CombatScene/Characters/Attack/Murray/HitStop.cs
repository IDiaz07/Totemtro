using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Stop(float duration)
    {
        StartCoroutine(StopRoutine(duration));
    }

    IEnumerator StopRoutine(float duration)
    {
        float original = Time.timeScale;

        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = original;
    }
}