using UnityEngine;

/// <summary>
/// Trackea daño producido y vida recuperada durante la run.
/// </summary>
public class CombatStatsTracker : MonoBehaviour
{
    public static CombatStatsTracker Instance;

    public static float TotalDamageDealt { get; private set; }
    public static float TotalHealthHealed { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void RegisterDamage(float amount)
    {
        TotalDamageDealt += amount;
    }

    public static void RegisterHealing(float amount)
    {
        TotalHealthHealed += amount;
    }

    public static void Reset()
    {
        TotalDamageDealt = 0f;
        TotalHealthHealed = 0f;
    }
}