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

        int timeScore = Mathf.FloorToInt(timeSurvived / 30f);
        int delta;

        if (extracted)
        {
            delta = 8 + timeScore;
        }
        else
        {
            delta = -6 + timeScore;
        }

        // 🧨 ANTI-GRINDEO (muertes rápidas)
        if (!extracted && timeSurvived < 30f)
        {
            delta -= 3;
        }

        // Clamp para estabilidad
        delta = Mathf.Clamp(delta, -12, 25);

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
        float xp = 0f;

        // Tiempo = base principal
        xp += timeSurvived * 1.2f;

        // Kills pesan menos (anti exploit)
        xp += kills * 0.5f;

        // ⭐ BONUS por performance real
        if (kills > 100)
        {
            xp += 50;
        }

        // 🎯 BONUS fuerte por victoria
        if (extracted)
        {
            xp += 120f;
        }
        else
        {
            xp *= 0.5f;
        }

        return Mathf.FloorToInt(Mathf.Max(5, xp));
    }
}