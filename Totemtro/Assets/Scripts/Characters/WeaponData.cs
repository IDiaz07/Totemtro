using UnityEngine;

public enum WeaponType
{
    Projectile,       // Disparo Tro y Orin
    MeleeArc,
    MurrayAnchor,     // Ancla especial de Murray
    GrimRuneBurst,    // Runas de Grim
    ConeProjectile,   // Disparo en cono (Vex)
    AreaZone,         // Zona en suelo (Selene)
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Aim Settings")]
    public float aimLength = 2f;
    public float aimAngle = 0f; // 0 = línea


    [Header("Visual")]
    public Sprite weaponSprite;

    // =====================================================
    // 📊 STATS GENERALES
    // =====================================================

    [Header("Base Stats")]
    public float damage = 10f;
    public float fireRate = 1f;

    // =====================================================
    // 🔫 PROYECTIL / CONO / GRIM
    // =====================================================

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float range = 10f;
    public int projectileCount = 1;
    public float spreadAngle = 30f;

    // =====================================================
    // ⚔️ MELEE
    // =====================================================

    [Header("Melee Settings")]
    public float meleeRadius = 1.5f;
    public float meleeAngle = 90f;

    // =====================================================
    // 🌿 AREA / ZONA
    // =====================================================

    [Header("Area Settings")]
    public float areaDuration = 3f;

    [Header("Grim Settings")]
    public float runeScale = 5f;
    public float runeRadius = 0.8f;

    [Header("Vex Settings")]
    int vexComboStep = 0;
    Vector2 lastVexDirection = Vector2.right;

    // =====================================================
    // ⚓ MURRAY SETTINGS
    // =====================================================

    [Header("Murray Settings")]
    public float murrayRadius = 0.5f;
    public float murrayOpenTime = 0.25f;
    public float murraySwingTime = 0.5f;
    public float murrayReturnTime = 0.25f;
    public float murrayConeAngle = 45f;
    public float murrayAnchorDamageRadius = 0.25f;
    public float murrayChainWidth = 0.15f;

}
