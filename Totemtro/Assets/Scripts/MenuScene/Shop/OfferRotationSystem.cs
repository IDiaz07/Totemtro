using UnityEngine;
using System;
using System.Collections.Generic;

public class OfferRotationSystem : MonoBehaviour
{
    public static OfferRotationSystem Instance;

    Dictionary<string, DateTime> offerEndTimes = new();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterLimitedItem(ShopItemData item)
    {
        if (!item.isLimited)
            return;

        if (!offerEndTimes.ContainsKey(item.itemId))
        {
            DateTime endTime = DateTime.Now.AddHours(item.durationHours);
            offerEndTimes[item.itemId] = endTime;
        }
    }

    public TimeSpan GetRemainingTime(string id)
    {
        if (!offerEndTimes.ContainsKey(id))
            return TimeSpan.Zero;

        return offerEndTimes[id] - DateTime.Now;
    }

    public bool IsExpired(string id)
    {
        return GetRemainingTime(id).TotalSeconds <= 0;
    }

    public void DailyRotation()
    {
        int seed = DateTime.Now.Day;
        UnityEngine.Random.InitState(seed);

        // ejemplo: marcar 3 items como featured
    }
}