using UnityEngine;

public static class GameInputLock
{
    static int lockCounter = 0;

    public static bool IsLocked => lockCounter > 0;

    public static void Lock()
    {
        lockCounter++;
    }

    public static void Unlock()
    {
        lockCounter--;

        if (lockCounter <= 0)
            lockCounter = 0;
    }

    public static void Reset()
    {
        lockCounter = 0;
    }
}
