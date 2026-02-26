using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    public Image fillImage;
    public PlayerExperience playerXP;

    void Update()
    {
        if (playerXP == null) return;

        fillImage.fillAmount = playerXP.GetXPPercent();
    }
}
