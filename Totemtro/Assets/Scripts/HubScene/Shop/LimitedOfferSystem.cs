using UnityEngine;
using System;

public class LimitedOfferSystem : MonoBehaviour
{
    public static LimitedOfferSystem Instance;

    void Awake()
    {
        Instance = this;
    }

    public void StartOffer(HeroType type, int hours)
    {
        DateTime endTime = DateTime.UtcNow.AddHours(hours);

        PlayerPrefs.SetString(
            $"Offer_{type}",
            endTime.ToBinary().ToString());

        PlayerPrefs.Save();
    }

    public bool IsOfferActive(HeroType type)
    {
        if (!PlayerPrefs.HasKey($"Offer_{type}"))
            return false;

        long binary =
            Convert.ToInt64(PlayerPrefs.GetString($"Offer_{type}"));

        DateTime endTime = DateTime.FromBinary(binary);

        return DateTime.UtcNow < endTime;
    }

    public TimeSpan GetRemainingTime(HeroType type)
    {
        if (!IsOfferActive(type))
            return TimeSpan.Zero;

        long binary =
            Convert.ToInt64(PlayerPrefs.GetString($"Offer_{type}"));

        DateTime endTime = DateTime.FromBinary(binary);

        return endTime - DateTime.UtcNow;
    }
}