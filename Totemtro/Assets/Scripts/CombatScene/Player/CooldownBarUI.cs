using UnityEngine;
using UnityEngine.UI;

public class CooldownBarUI : MonoBehaviour
{
    public Image fillImage;
    public Weapon weapon;

    void Update()
    {
        if (weapon == null) return;

        if (weapon.CurrentCooldownDuration <= 0f)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        float ratio =
            weapon.CooldownRemaining /
            weapon.CurrentCooldownDuration;

        fillImage.fillAmount = ratio;
    }
}