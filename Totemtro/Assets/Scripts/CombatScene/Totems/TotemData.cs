using UnityEngine;

[System.Flags]
public enum TotemTargetType
{
    None = 0,
    Vex = 1 << 0,
    Murray = 1 << 1,
    Kael = 1 << 2,
    Grim = 1 << 3,
    Orin = 1 << 4,
    Nyra = 1 << 5,
    Tro = 1 << 6,
    Selene = 1 << 7,

    All = ~0
}

public enum TotemType
{
    DualFire,
    TripleShot,
    RapidBullets,
    Piercing,
    Ricochet,
    Vitality,
    Power,
    Swiftness,
    Recovery,
    Shielding,
    Evasion,
    Fortitude,
    SecondWind,
    Dash,
    BloodPrice,
    Retaliation
}

public enum TotemRarity
{
    Common,
    Rare,
    Legendary
}

[CreateAssetMenu(fileName = "NewTotem", menuName = "Game/Totem")]
public class TotemData : ScriptableObject
{
    [Header("Basic Info")]
    public string totemName;

    [TextArea]
    public string description;

    [Header("Economy")]
    public int price = 50;

    [Header("Visual")]
    public Sprite icon;

    [Header("Type")]
    public TotemType totemType;

    [Header("Rarity")]
    public TotemRarity rarity;

    [Header("Target Heroes")]
    public TotemTargetType targetHeroes = TotemTargetType.All;
}

