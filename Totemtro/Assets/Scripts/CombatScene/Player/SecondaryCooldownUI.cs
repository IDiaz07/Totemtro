using UnityEngine;
using UnityEngine.UI;

public class SecondaryCooldownUI : MonoBehaviour
{
    public Image fillImage;
    public Weapon weapon;

    void Update()
    {
        if (weapon == null || weapon.currentWeapon == null)
            return;

        bool hasSecondary =
            weapon.currentWeapon.weaponType == WeaponType.MurrayAnchor ||
            weapon.currentWeapon.weaponType == WeaponType.KaelBlade;

        // activar / desactivar UI
        gameObject.SetActive(hasSecondary);

        if (!hasSecondary) return;

        if (weapon.SecondaryCooldownDuration <= 0f)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        float ratio =
            weapon.SecondaryCooldownRemaining /
            weapon.SecondaryCooldownDuration;

        fillImage.fillAmount = ratio;
    }
}