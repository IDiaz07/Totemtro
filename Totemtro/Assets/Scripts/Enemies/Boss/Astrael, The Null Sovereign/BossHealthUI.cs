using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    public Image healthFill;
    public TMP_Text healthText;
    public TMP_Text bossNameText;

    Enemy boss;

    public void Init(Enemy enemy, string bossName)
    {
        boss = enemy;
        bossNameText.text = bossName;
    }

    void Update()
    {
        if (boss == null) return;

        float percent =
            boss.GetCurrentHealth() / boss.maxHealth;

        healthFill.fillAmount = percent;

        healthText.text =
            Mathf.CeilToInt(boss.GetCurrentHealth()) +
            " / " +
            Mathf.CeilToInt(boss.maxHealth);
    }
}
