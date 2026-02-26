using UnityEngine;

public static class GamePause
{
    static int pauseCounter = 0;

    public static void Pause()
    {
        pauseCounter++;
        Time.timeScale = 0f;
    }

    public static void Resume()
    {
        pauseCounter--;
        if (pauseCounter <= 0)
        {
            pauseCounter = 0;
            Time.timeScale = 1f;
        }
    }

    public static void Reset()
    {
        pauseCounter = 0;
        Time.timeScale = 1f;
    }
}

