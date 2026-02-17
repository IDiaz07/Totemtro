using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthFill;
    public TMP_Text healthText;
    public TMP_Text bossNameText;
    public CanvasGroup canvasGroup;   // 👈 FALTABA ESTO

    Enemy boss;

    public void Init(Enemy enemy, string bossName)
    {
        boss = enemy;
        bossNameText.text = bossName;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (boss == null)
        {
            StartCoroutine(FadeOutAndDestroy());
            return;
        }

        float percent =
            boss.GetCurrentHealth() / boss.maxHealth;

        healthFill.fillAmount = percent;

        healthText.text =
            Mathf.CeilToInt(boss.GetCurrentHealth()) +
            " / " +
            Mathf.CeilToInt(boss.maxHealth);
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndDestroy()
    {
        if (canvasGroup == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float t = 0f;
        float duration = 0.6f;

        while (t < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
