using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LimitedHeroOfferSystem : MonoBehaviour
{
    public static LimitedHeroOfferSystem Instance;

    const int OFFER_DURATION_DAYS = 7;
    const string PREF_END_DATE = "LimitedHeroOffer_EndDate";
    const string PREF_HERO_LIST = "LimitedHeroOffer_HeroList";

    List<HeroType> currentOffers = new();

    bool initialized = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PlayerPrefs.DeleteKey("LimitedHeroOffer_EndDate");
        PlayerPrefs.DeleteKey("LimitedHeroOffer_HeroList");
        PlayerPrefs.Save();
        StartCoroutine(InitializeWhenReady());
    }

    IEnumerator InitializeWhenReady()
    {
        yield return new WaitUntil(() =>
            HeroProgressSystem.Instance != null &&
            HeroProgressSystem.Instance.GetAllHeroes() != null &&
            HeroProgressSystem.Instance.GetAllHeroes().Count > 0);

        InitializeOffer();
        initialized = true;

        Debug.Log("[OFFER] System initialized");
    }

    void InitializeOffer()
    {
        if (HasSavedOffer())
            LoadOffer();
        else
            GenerateNewOffer();
    }

    bool HasSavedOffer()
    {
        return PlayerPrefs.HasKey(PREF_END_DATE) &&
               PlayerPrefs.HasKey(PREF_HERO_LIST);
    }

    void LoadOffer()
    {
        long binary = Convert.ToInt64(PlayerPrefs.GetString(PREF_END_DATE));
        DateTime endDate = DateTime.FromBinary(binary);

        if (DateTime.UtcNow > endDate)
        {
            GenerateNewOffer();
            return;
        }

        string saved = PlayerPrefs.GetString(PREF_HERO_LIST);
        string[] split = saved.Split(',');

        currentOffers.Clear();

        foreach (var s in split)
        {
            if (Enum.TryParse(s, out HeroType type))
                currentOffers.Add(type);
        }

        if (currentOffers.Count == 0)
            GenerateNewOffer();
    }

    void GenerateNewOffer()
    {
        Debug.Log("[OFFER] Generating new weekly offer");

        var heroes = HeroProgressSystem.Instance.GetAllHeroes();

        if (heroes == null || heroes.Count == 0)
        {
            Debug.LogError("[OFFER] No heroes available");
            return;
        }

        currentOffers.Clear();

        List<HeroData> allHeroes = new List<HeroData>(heroes);

        // 🔥 IMPORTANTE: NO eliminar desbloqueados en desarrollo
        // Si quieres puedes excluir max level, pero no desbloqueados

        // Shuffle
        for (int i = 0; i < allHeroes.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, allHeroes.Count);
            var temp = allHeroes[i];
            allHeroes[i] = allHeroes[rand];
            allHeroes[rand] = temp;
        }

        int count = Mathf.Min(3, allHeroes.Count);

        for (int i = 0; i < count; i++)
            currentOffers.Add(allHeroes[i].heroType);

        Debug.Log($"[OFFER] Final offers count: {currentOffers.Count}");

        SaveOffer();
    }

    void SaveOffer()
    {
        DateTime endDate = DateTime.UtcNow.AddDays(OFFER_DURATION_DAYS);

        PlayerPrefs.SetString(
            PREF_END_DATE,
            endDate.ToBinary().ToString());

        string joined = string.Join(",", currentOffers);
        PlayerPrefs.SetString(PREF_HERO_LIST, joined);

        PlayerPrefs.Save();
    }

    public bool IsHeroOnOffer(HeroType type)
    {
        if (!initialized)
            return false;

        return currentOffers.Contains(type);
    }

    public TimeSpan GetRemainingTime()
    {
        if (!PlayerPrefs.HasKey(PREF_END_DATE))
            return TimeSpan.Zero;

        long binary =
            Convert.ToInt64(PlayerPrefs.GetString(PREF_END_DATE));

        DateTime endDate = DateTime.FromBinary(binary);

        return endDate - DateTime.UtcNow;
    }

    public int GetDiscountPercent()
    {
        return 25;
    }
}