using UnityEngine;

public static class GamePause
{
    static int pauseCounter = 0;

    public static bool IsPaused => pauseCounter > 0;

    public static void Pause()
    {
        pauseCounter++;

        if (pauseCounter == 1)
            Time.timeScale = 0f;
    }

    public static void Resume()
    {
        if (pauseCounter <= 0)
        {
            Reset();
            return;
        }

        pauseCounter--;

        if (pauseCounter == 0)
            Time.timeScale = 1f;
    }

    public static void Reset()
    {
        pauseCounter = 0;
        Time.timeScale = 1f;
    }
}