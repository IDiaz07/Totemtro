using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomTooltipUI : MonoBehaviour
{
    [Header("Main")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description;

    [Header("Stats")]
    public GameObject damageStat;
    public TextMeshProUGUI damageText;

    public GameObject dpsStat;
    public TextMeshProUGUI dpsText;

    public GameObject finalDamageStat;
    public TextMeshProUGUI finalDamageText;

    public GameObject healStat;
    public TextMeshProUGUI healText;

    public GameObject resistanceStat;
    public TextMeshProUGUI resistanceText;

    public GameObject durabilityStat;
    public TextMeshProUGUI durabilityText;

    // =========================
    public void Show(ItemData item)
    {
        if (item == null) return;

        gameObject.SetActive(true);

        itemName.text = item.itemName;
        description.text = item.description;

        // DAMAGE
        damageStat.SetActive(item.showDamage);
        if (item.showDamage)
            damageText.text = item.damage.ToString();

        // DPS
        dpsStat.SetActive(item.showDPS);
        if (item.showDPS)
            dpsText.text = item.damagePerSecond.ToString();

        // FINAL DAMAGE
        finalDamageStat.SetActive(item.showFinalDamage);
        if (item.showFinalDamage)
            finalDamageText.text = item.finalDamage.ToString();

        // HEAL
        healStat.SetActive(item.showHealing);
        if (item.showHealing)
            healText.text = item.healingAmount.ToString();

        // RESISTANCE
        bool hasRes = item.damageReduction > 0;
        resistanceStat.SetActive(hasRes);
        if (hasRes)
            resistanceText.text = Mathf.RoundToInt(item.damageReduction * 100) + "%";

        // DURABILITY
        bool hasDur = item.maxDurability > 0;
        durabilityStat.SetActive(hasDur);
        if (hasDur)
            durabilityText.text = item.maxDurability.ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}