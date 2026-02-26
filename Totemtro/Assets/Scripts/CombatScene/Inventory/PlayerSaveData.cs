using System;

[Serializable]
public class SlotSaveData
{
    public string id;
    public int amount;
}

[System.Serializable]
public class PlayerSaveData
{
    public SlotSaveData[] inventory;
    public int[] actionBarIndices;
}