using System;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    public static event Action OnReady;

    ISaveSystem saveSystem;

    // Indica que el sistema está listo
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

        saveSystem = new LocalSaveSystem();

        IsReady = true;
        Debug.Log("SaveSystem initialized");
        OnReady?.Invoke();
    }

    public void Save(string key, string data) => saveSystem.Save(key, data);
    public string Load(string key) => saveSystem.Load(key);
}

public class MetaInventoryDiagnostics : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(LogStatus), 0.5f);
    }

    void LogStatus()
    {
        Debug.Log("=== MetaInventory Diagnostics ===");
        Debug.Log("SaveSystem.Instance: " + (SaveSystem.Instance != null));
        Debug.Log("SaveSystem.IsReady: " + (SaveSystem.Instance != null ? SaveSystem.Instance.IsReady.ToString() : "n/a"));
        Debug.Log("ItemDatabase.Instance: " + (ItemDatabase.Instance != null));
        Debug.Log("MetaInventory.Instance: " + (MetaInventory.Instance != null));
        if (MetaInventory.Instance != null)
        {
            Debug.Log("MetaInventory.IsInitialized: " + MetaInventory.Instance.IsInitialized);
            Debug.Log("MetaInventory.slots: " + (MetaInventory.Instance.slots != null ? MetaInventory.Instance.slots.Length.ToString() : "null"));
        }
    }
}