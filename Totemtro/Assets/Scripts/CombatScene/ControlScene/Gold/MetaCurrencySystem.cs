using UnityEngine;

public class MetaCurrencySystem : MonoBehaviour
{
    public static MetaCurrencySystem Instance;

    public int MetaGold { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void AddMetaGold(int amount)
    {
        MetaGold += amount;
        Save();
    }

    void Save()
    {
        PlayerPrefs.SetInt("MetaGold", MetaGold);
    }

    void Load()
    {
        MetaGold = PlayerPrefs.GetInt("MetaGold", 0);
    }

    public void Add(int amount)
    {
        MetaGold += amount;
        PlayerPrefs.SetInt("MetaGold", MetaGold);
        PlayerPrefs.Save();
    }

    public bool Spend(int amount)
    {
        if (MetaGold < amount)
            return false;

        MetaGold -= amount;
        PlayerPrefs.SetInt("MetaGold", MetaGold);
        PlayerPrefs.Save();
        return true;
    }
}