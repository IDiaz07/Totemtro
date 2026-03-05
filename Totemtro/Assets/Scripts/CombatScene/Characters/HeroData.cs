using UnityEngine;
using System.Collections.Generic;

public enum HeroType
{
    Vex,
    Murray,
    Kael,
    Grim,
    Orin,
    Nyra,
    Tro,
    Selene
}

public enum HeroRarity
{
    Rare,
    Epic,
    Legendary
}

public enum HeroRole
{
    Melee,
    Ranged,
    Support,
    Tank,
    Assassin,
    Mage
}

[System.Serializable]
public class HeroDirectionalSprites
{
    public Sprite FrontView;
    public Sprite BackView;
    public Sprite LeftView;
    public Sprite RightView;

    public Sprite FrontLeftView;
    public Sprite FrontRightView;
    public Sprite BackLeftView;
    public Sprite BackRightView;
}

[CreateAssetMenu(fileName = "NewHero", menuName = "Game/Hero")]
public class HeroData : ScriptableObject
{
    [Header("Identity")]
    public HeroType heroType;

    [Header("Info")]
    public string heroName;

    [TextArea]
    public string description;

    [Header("UI Icons")]
    public Sprite Icon;
    public Sprite ChampsHeadIcon;

    [Header("Role")]
    public HeroRole role;

    [Header("Rarity")]
    public HeroRarity rarity;

    [Header("Directional Sprites")]
    public HeroDirectionalSprites directionalSprites;

    [Header("Base Stats")]
    public float maxHealth;
    public float damage;
    public float moveSpeed;
    public float fireRate;

    [Header("Weapon")]
    public List<WeaponData> weapons;

    [Header("Unlock")]
    public bool unlockedByDefault = true;
    public int gemCost = 0;

    [Header("Level Scaling (0–9)")]
    public AnimationCurve healthScaling = AnimationCurve.Linear(1, 1, 9, 1.5f);
    public AnimationCurve damageScaling = AnimationCurve.Linear(1, 1, 9, 1.5f);
    public AnimationCurve speedScaling = AnimationCurve.Linear(1, 1, 9, 1.2f);
    public AnimationCurve fireRateScaling = AnimationCurve.Linear(1, 1, 9, 1.3f);

    [Header("Limited Offer")]
    public bool hasLimitedOffer;
    public int limitedDiscountPercent;
    public int limitedDurationHours;
}