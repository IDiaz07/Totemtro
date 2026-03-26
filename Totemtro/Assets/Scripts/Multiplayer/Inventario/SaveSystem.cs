using System;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    public static event Action OnReady;

    ISaveSystem saveSystem;

    public bool IsReady { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Initialize()
    {
        saveSystem = new LocalSaveSystem();

        IsReady = true;

        Debug.Log("✅ SaveSystem initialized");

        OnReady?.Invoke();
    }

    // ===============================
    // SAVE
    // ===============================

    public void Save(string key, string data)
    {
        if (!IsReady)
        {
            Debug.LogWarning("⚠️ SaveSystem not ready");
            return;
        }

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("❌ Save key is null or empty");
            return;
        }

        saveSystem.Save(key, data);
    }

    // ===============================
    // LOAD
    // ===============================

    public string Load(string key)
    {
        if (!IsReady)
        {
            Debug.LogWarning("⚠️ SaveSystem not ready");
            return null;
        }

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("❌ Load key is null or empty");
            return null;
        }

        return saveSystem.Load(key);
    }
}