using UnityEngine;
using System;

public class PlayerShopProfileSystem : MonoBehaviour
{
    public static PlayerShopProfileSystem Instance;

    // ==============================
    // METRICS
    // ==============================

    public int TotalPurchases { get; private set; }
    public int TotalSpentGold { get; private set; }
    public int TotalSpentGems { get; private set; }

    public int SessionsWithoutPurchase { get; private set; }

    DateTime lastPurchaseTime;

    const string KEY_TOTAL_PURCHASES = "PS_TotalPurchases";
    const string KEY_TOTAL_GOLD = "PS_TotalGold";
    const string KEY_TOTAL_GEMS = "PS_TotalGems";
    const string KEY_SESSIONS_WITHOUT = "PS_SessionsWithout";
    const string KEY_LAST_PURCHASE = "PS_LastPurchaseTime";

    // ==============================
    // INIT
    // ==============================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null); // 👈 CLAVE
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void Load()
    {
        TotalPurchases = PlayerPrefs.GetInt(KEY_TOTAL_PURCHASES, 0);
        TotalSpentGold = PlayerPrefs.GetInt(KEY_TOTAL_GOLD, 0);
        TotalSpentGems = PlayerPrefs.GetInt(KEY_TOTAL_GEMS, 0);
        SessionsWithoutPurchase = PlayerPrefs.GetInt(KEY_SESSIONS_WITHOUT, 0);

        string savedTime = PlayerPrefs.GetString(KEY_LAST_PURCHASE, "");

        if (!string.IsNullOrEmpty(savedTime))
            lastPurchaseTime = DateTime.Parse(savedTime);
    }

    void Save()
    {
        PlayerPrefs.SetInt(KEY_TOTAL_PURCHASES, TotalPurchases);
        PlayerPrefs.SetInt(KEY_TOTAL_GOLD, TotalSpentGold);
        PlayerPrefs.SetInt(KEY_TOTAL_GEMS, TotalSpentGems);
        PlayerPrefs.SetInt(KEY_SESSIONS_WITHOUT, SessionsWithoutPurchase);
        PlayerPrefs.SetString(KEY_LAST_PURCHASE, lastPurchaseTime.ToString());
        PlayerPrefs.Save();
    }

    // ==============================
    // REGISTER PURCHASE
    // ==============================

    public void RegisterPurchase(int goldSpent, int gemsSpent)
    {
        TotalPurchases++;

        TotalSpentGold += goldSpent;
        TotalSpentGems += gemsSpent;

        SessionsWithoutPurchase = 0;

        lastPurchaseTime = DateTime.Now;

        Save();
    }

    public void RegisterSessionWithoutPurchase()
    {
        SessionsWithoutPurchase++;
        Save();
    }

    // ==============================
    // BEHAVIOR FLAGS
    // ==============================

    public bool HasMadeFirstPurchase()
    {
        return TotalPurchases > 0;
    }

    public bool IsWhale()
    {
        return TotalSpentGems >= 5000;
    }

    public bool IsDormantUser()
    {
        if (lastPurchaseTime == default)
            return true;

        return (DateTime.Now - lastPurchaseTime).TotalDays >= 7;
    }

    public bool NeedsRetentionOffer()
    {
        return SessionsWithoutPurchase >= 3;
    }

    // ==============================
    // DEBUG RESET (OPTIONAL)
    // ==============================

    public void ResetProfile()
    {
        PlayerPrefs.DeleteKey(KEY_TOTAL_PURCHASES);
        PlayerPrefs.DeleteKey(KEY_TOTAL_GOLD);
        PlayerPrefs.DeleteKey(KEY_TOTAL_GEMS);
        PlayerPrefs.DeleteKey(KEY_SESSIONS_WITHOUT);
        PlayerPrefs.DeleteKey(KEY_LAST_PURCHASE);

        Load();
    }

    public bool IsF2P()
    {
        return !HasMadeFirstPurchase();
    }
}