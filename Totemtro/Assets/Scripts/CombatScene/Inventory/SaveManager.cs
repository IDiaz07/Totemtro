using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public RunInventory inventory;
    public ActionBarController actionBar;

    public void Save()
    {
        PlayerSaveData data =
            inventory.CreateSaveData(actionBar);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PlayerSave", json);
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey("PlayerSave"))
            return;

        string json =
            PlayerPrefs.GetString("PlayerSave");

        PlayerSaveData data =
            JsonUtility.FromJson<PlayerSaveData>(json);

        inventory.LoadFromSave(data, actionBar);
    }
}