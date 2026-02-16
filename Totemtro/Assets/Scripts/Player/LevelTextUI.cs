using UnityEngine;
using TMPro;

public class LevelTextUI : MonoBehaviour
{
    public PlayerExperience playerXP;
    public TextMeshProUGUI levelText;

    void Update()
    {
        if (playerXP == null) return;

        levelText.text = "LV " + playerXP.currentLevel.ToString();
    }
}
