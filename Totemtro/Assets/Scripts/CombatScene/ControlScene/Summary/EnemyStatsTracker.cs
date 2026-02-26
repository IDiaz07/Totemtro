using UnityEngine;

public class EnemyStatsTracker : MonoBehaviour
{
    public static int Kills { get; private set; }

    public static void RegisterKill()
    {
        Kills++;
    }

    public static void Reset()
    {
        Kills = 0;
    }
}