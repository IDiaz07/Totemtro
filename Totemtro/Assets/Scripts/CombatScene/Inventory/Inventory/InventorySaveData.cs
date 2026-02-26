using System;

[Serializable]
public class InventoryItemSave
{
    public string id;
    public int amount;
}

[Serializable]
public class InventorySaveData
{
    public InventoryItemSave[] items;
}