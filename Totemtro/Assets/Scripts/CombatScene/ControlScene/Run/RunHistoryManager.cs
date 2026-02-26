using UnityEngine;
using System;

public class RunHistoryManager : MonoBehaviour
{
    public static RunHistoryManager Instance;

    RunHistoryContainer container;

    const string SAVE_KEY = "RunHistory";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void AddRun(
        float time,
        int kills,
        int gold,
        bool extracted)
    {
        RunHistoryEntry entry =
            new RunHistoryEntry();

        entry.timeSurvived = time;
        entry.enemiesKilled = kills;
        entry.goldEarned = gold;
        entry.extracted = extracted;
        entry.date = DateTime.Now.ToString();

        container.runs.Add(entry);

        Save();
    }

    void Save()
    {
        string json =
            JsonUtility.ToJson(container);

        PlayerPrefs.SetString(SAVE_KEY, json);
    }

    void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            container = new RunHistoryContainer();
            return;
        }

        string json =
            PlayerPrefs.GetString(SAVE_KEY);

        container =
            JsonUtility.FromJson<RunHistoryContainer>(json);
    }

    public RunHistoryContainer GetHistory()
    {
        return container;
    }
}