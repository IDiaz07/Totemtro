using UnityEngine;

/// <summary>
/// Gestiona copas (trophies) y maestría por héroe.
/// Persiste con PlayerPrefs.
/// </summary>
public class HeroMasterySystem : MonoBehaviour
{
    public static HeroMasterySystem Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================================
    // TROPHIES (COPAS)
    // =========================================

    string TrophyKey(HeroType hero) => "Trophies_" + hero;

    public int GetTrophies(HeroType hero)
    {
        return PlayerPrefs.GetInt(TrophyKey(hero), 0);
    }

    public void SetTrophies(HeroType hero, int value)
    {
        value = Mathf.Max(0, value);
        PlayerPrefs.SetInt(TrophyKey(hero), value);
        PlayerPrefs.Save();
    }

    public int ApplyTrophyResult(HeroType hero, bool extracted, float timeSurvived, int kills)
    {
        int current = GetTrophies(hero);
        int delta;

        if (extracted)
        {
            int timeBonus = Mathf.FloorToInt(timeSurvived / 30f);
            int killBonus = Mathf.FloorToInt(kills / 5f);
            delta = 5 + timeBonus + killBonus;
            delta = Mathf.Clamp(delta, 5, 40);
        }
        else
        {
            int timeReduce = Mathf.FloorToInt(timeSurvived / 60f);
            int killReduce = Mathf.FloorToInt(kills / 10f);
            delta = -(8 - timeReduce - killReduce);
            delta = Mathf.Clamp(delta, -8, -1);
        }

        int newValue = Mathf.Max(0, current + delta);
        SetTrophies(hero, newValue);

        return delta;
    }

    // =========================================
    // MASTERY XP
    // =========================================

    string MasteryKey(HeroType hero) => "MasteryXP_" + hero;

    public int GetMasteryXP(HeroType hero)
    {
        return PlayerPrefs.GetInt(MasteryKey(hero), 0);
    }

    public void AddMasteryXP(HeroType hero, int amount)
    {
        int current = GetMasteryXP(hero);
        int newXP = current + Mathf.Max(0, amount);
        PlayerPrefs.SetInt(MasteryKey(hero), newXP);
        PlayerPrefs.Save();
    }

    public MasteryTier GetMasteryTier(HeroType hero)
    {
        return HeroData.GetTierFromXP(GetMasteryXP(hero));
    }

    public float GetMasteryProgress(HeroType hero)
    {
        return HeroData.GetTierProgress(GetMasteryXP(hero));
    }

    public int CalculateMasteryXP(bool extracted, float timeSurvived, int kills)
    {
        float baseXP = timeSurvived * 0.5f + kills * 2f;

        if (extracted)
            baseXP *= 1.5f;
        else
            baseXP *= 0.6f;

        return Mathf.FloorToInt(Mathf.Max(1, baseXP));
    }
}