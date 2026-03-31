using UnityEngine;

[System.Serializable]
public class SlotIconData
{
    public SlotIconType type;
    public Sprite sprite;
}

public enum SlotIconType
{
    Loss,
    Common,
    Rare,
    Epic,
    Legendary
}