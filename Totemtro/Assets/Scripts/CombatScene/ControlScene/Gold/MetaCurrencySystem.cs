using UnityEngine;
using System;

public class MetaCurrencySystem : MonoBehaviour
{
    public static MetaCurrencySystem Instance;

    public int Gold { get; private set; }
    public int Gems { get; private set; }

    public static Action OnCurrencyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    void Load()
    {
        Gold = PlayerPrefs.GetInt("MetaGold", 0);
        Gems = PlayerPrefs.GetInt("MetaGems", 0);
    }

    void Save()
    {
        PlayerPrefs.SetInt("MetaGold", Gold);
        PlayerPrefs.SetInt("MetaGems", Gems);
        PlayerPrefs.Save();
    }

    // =====================================================
    // ADD WITH ANIMATION (SHOP)
    // =====================================================

    public void AddGold(int amount, Vector3 worldStartPos)
    {
        Gold += amount;
        Save();

        CurrencyAnimationSystem.Instance
            ?.PlayGoldAnimation(amount, worldStartPos);

        // 🚫 NO OnCurrencyChanged aquí
    }

    public void AddGems(int amount, Vector3 worldStartPos)
    {
        Gems += amount;
        Save();

        CurrencyAnimationSystem.Instance
            ?.PlayGemsAnimation(amount, worldStartPos);

        // 🚫 NO OnCurrencyChanged aquí
    }

    // =====================================================
    // ADD WITHOUT ANIMATION (IAP / DEBUG / COMBAT)
    // =====================================================

    public void AddGold(int amount)
    {
        Gold += amount;
        Save();
        OnCurrencyChanged?.Invoke(); // solo aquí
    }

    public void AddGems(int amount)
    {
        Gems += amount;
        Save();
        OnCurrencyChanged?.Invoke(); // solo aquí
    }

    // =====================================================
    // SPEND
    // =====================================================

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;

        Gold -= amount;
        Save();
        OnCurrencyChanged?.Invoke();
        return true;
    }

    public bool SpendGems(int amount)
    {
        if (Gems < amount) return false;

        Gems -= amount;
        Save();
        OnCurrencyChanged?.Invoke();
        return true;
    }

    // =====================================================
    // LEGACY
    // =====================================================

    public int MetaGold => Gold;

    public void AddMetaGold(int amount)
    {
        AddGold(amount);
    }

    public void Add(int amount)
    {
        AddGold(amount);
    }

    public bool Spend(int amount)
    {
        return SpendGold(amount);
    }
}