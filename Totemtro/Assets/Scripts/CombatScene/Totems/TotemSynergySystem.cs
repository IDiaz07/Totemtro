using UnityEngine;

public class TotemSynergySystem : MonoBehaviour
{
    TotemInventory inventory;
    Weapon weapon;

    bool chaosMirrorActive = false;
    float chaosMirrorBonus = 1.25f;

    void Awake()
    {
        inventory = GetComponent<TotemInventory>();
        weapon = GetComponentInChildren<Weapon>();
    }

    public void CheckSynergies()
    {
        if (inventory == null || weapon == null || weapon.currentWeapon == null)
            return;

        bool hasDual = inventory.HasTotem(TotemType.DualFire);
        bool hasRicochet = inventory.HasTotem(TotemType.Ricochet);

        // ===============================
        // CHAOS MIRROR SYNERGY
        // Dual Fire + Ricochet
        // ===============================
        if (hasDual && hasRicochet && !chaosMirrorActive)
        {
            ActivateChaosMirror();
        }
        else if ((!hasDual || !hasRicochet) && chaosMirrorActive)
        {
            DeactivateChaosMirror();
        }
    }

    void ActivateChaosMirror()
    {
        chaosMirrorActive = true;

        weapon.currentWeapon.damage *= chaosMirrorBonus;

        Debug.Log("🔥 SYNERGY ACTIVATED: CHAOS MIRROR");
    }

    void DeactivateChaosMirror()
    {
        chaosMirrorActive = false;

        weapon.currentWeapon.damage /= chaosMirrorBonus;

        Debug.Log("❌ SYNERGY DEACTIVATED: CHAOS MIRROR");
    }
}
