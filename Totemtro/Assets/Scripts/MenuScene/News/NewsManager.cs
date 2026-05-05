using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewsManager : MonoBehaviour
{
    public static NewsManager Instance;

    [SerializeField] private NewsDatabase database;

    private const string PREF_KEY = "SEEN_NEWS";

    private HashSet<string> seenNews = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSeenNews();

        Debug.Log("🟢 NewsManager inicializado");
    }

    public List<NewsData> GetSortedNews()
    {
        return database.newsList
            .OrderByDescending(n => n.date)
            .ToList();
    }

    public bool IsSeen(string id)
    {
        return seenNews.Contains(id);
    }

    public void MarkAsSeen(string id)
    {
        if (seenNews.Add(id))
            SaveSeenNews();
    }

    private void LoadSeenNews()
    {
        string data = PlayerPrefs.GetString(PREF_KEY, "");
        seenNews = new HashSet<string>(data.Split('|'));
    }

    private void SaveSeenNews()
    {
        string data = string.Join("|", seenNews);
        PlayerPrefs.SetString(PREF_KEY, data);
    }

    public bool HasUnread()
    {
        return database.newsList.Any(n => !IsSeen(n.id));
    }
}