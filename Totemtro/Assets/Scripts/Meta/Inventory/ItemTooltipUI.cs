using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance;

    public GameObject panel;
    RectTransform panelRect;

    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description;

    public GameObject damageLine;
    public GameObject dpsLine;
    public GameObject finalDamageLine;
    public GameObject healLine;

    public TextMeshProUGUI damageText;
    public TextMeshProUGUI dpsText;
    public TextMeshProUGUI finalDamageText;
    public TextMeshProUGUI healText;

    [Header("Layout")]
    public float startX = -50f;
    public float spacing = 100f;

    void Awake()
    {
        Instance = this;
        panelRect = panel.GetComponent<RectTransform>();
        panel.SetActive(false);
    }

    // ===============================
    // SHOW TOOLTIP
    // ===============================

    public void Show(ItemData item, RectTransform slot)
    {
        panel.SetActive(true);

        itemName.text = item.itemName;
        description.text = item.description;

        damageLine.SetActive(false);
        dpsLine.SetActive(false);
        finalDamageLine.SetActive(false);
        healLine.SetActive(false);

        List<GameObject> activeStats = new List<GameObject>();

        if (item.showHealing)
        {
            healLine.SetActive(true);
            healText.text = item.healingAmount.ToString();
            activeStats.Add(healLine);
        }

        if (item.showFinalDamage)
        {
            finalDamageLine.SetActive(true);
            finalDamageText.text = item.finalDamage.ToString();
            activeStats.Add(finalDamageLine);
        }

        if (item.showDPS)
        {
            dpsLine.SetActive(true);
            dpsText.text = item.damagePerSecond + " DPS";
            activeStats.Add(dpsLine);
        }

        if (item.showDamage)
        {
            damageLine.SetActive(true);
            damageText.text = item.damage.ToString();
            activeStats.Add(damageLine);
        }

        LayoutStats(activeStats);

        PositionTooltip(slot);
    }

    // ===============================
    // STATS POSITION
    // ===============================

    void LayoutStats(List<GameObject> stats)
    {
        float posX = startX;

        foreach (var stat in stats)
        {
            RectTransform rt = stat.GetComponent<RectTransform>();

            Vector2 pos = rt.anchoredPosition;
            pos.x = posX;
            rt.anchoredPosition = pos;

            posX -= spacing;
        }
    }

    // ===============================
    // TOOLTIP POSITION
    // ===============================

    void PositionTooltip(RectTransform slot)
    {
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        slot.GetWorldCorners(corners);

        Vector3 topRight = corners[2];
        Vector3 topLeft = corners[1];

        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            topRight,
            canvas.worldCamera,
            out pos
        );

        float tooltipWidth = panelRect.rect.width;

        // comprobar si cabe a la derecha
        if (topRight.x + tooltipWidth < Screen.width)
        {
            panelRect.anchoredPosition = pos + new Vector2(25, -10);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                topLeft,
                canvas.worldCamera,
                out pos
            );

            panelRect.anchoredPosition = pos + new Vector2(-tooltipWidth - 25, -10);
        }
    }

    // ===============================
    // HIDE
    // ===============================

    public void Hide()
    {
        panel.SetActive(false);
    }
}
