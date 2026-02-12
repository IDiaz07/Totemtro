using UnityEngine;

[CreateAssetMenu(fileName = "NewHero", menuName = "Game/Hero")]
public class HeroData : ScriptableObject
{
    [Header("Info")]
    public string heroName;
    [TextArea] public string description;
    public Sprite portrait;

    [Header("Visual")]
    public Sprite bodySprite;

    [Header("Base Stats")]
    public float maxHealth;
    public float damage;
    public float moveSpeed;
    public float fireRate;

    [Header("Weapon")]
    public WeaponData startingWeapon;

    [Header("Unlock")]
    public bool unlockedByDefault;
    public int gemCost;
}
