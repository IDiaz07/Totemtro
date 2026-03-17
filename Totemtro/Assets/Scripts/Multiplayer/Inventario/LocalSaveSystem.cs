using UnityEngine;

public class LocalSaveSystem : ISaveSystem
{
    public void Save(string key, string data)
    {
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();

        Debug.Log("Saved: " + key);
    }

    public string Load(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log("No save found for: " + key);
            return null;
        }

        string data = PlayerPrefs.GetString(key);

        Debug.Log("Loaded: " + key);

        return data;
    }
}