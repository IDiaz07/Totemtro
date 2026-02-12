using UnityEngine;

public enum WeaponType
{
    Projectile,
    MeleeArc,
    ConeProjectile,
    AreaZone,
    MultiWave
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;

    [Header("Visual")]
    public Sprite weaponSprite;

    [Header("Stats")]
    public float damage;
    public float fireRate;

    [Header("Projectile Settings")]
    public float projectileSpeed;
    public float range;
    public int projectileCount;
    public float spreadAngle;

    [Header("Melee Settings")]
    public float meleeRadius;

    [Header("Area Settings")]
    public float areaDuration;
}
