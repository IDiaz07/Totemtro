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

[CreateAssetMenu(fileName = "NewHero", menuName = "Game/Hero")]
public class HeroData : ScriptableObject
{
    [Header("Identity")]
    public HeroType heroType;

    [Header("Info")]
    public string heroName;

    [TextArea]
    public string description;

    public Sprite portrait;

    [Header("Visual")]
    public Sprite bodySprite;

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
}
