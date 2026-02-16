using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TotemTooltipUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject root;                 // Objeto visual del tooltip
    RectTransform rootRect;                 // RectTransform REAL del tooltip

    [Header("Header")]
    public Image iconImage;
    public TMP_Text nameText;

    [Header("Content")]
    public TMP_Text descriptionText;
    public TMP_Text buffText;
    public TMP_Text comparisonText;

    [Header("Visual")]
    public Image background;
    public Image glow;

    CanvasGroup canvasGroup;
    RectTransform canvasRect;

    PlayerStats stats;
    HeroController hero;

    Vector2 padding = new Vector2(20f, 20f);

    void Awake()
    {
        canvasGroup = root.GetComponent<CanvasGroup>();
        rootRect = root.GetComponent<RectTransform>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        stats = FindFirstObjectByType<PlayerStats>();
        hero = FindFirstObjectByType<HeroController>();

        HideInstant();
    }

    void Update()
    {
        if (!root.activeSelf) return;
        FollowMouse();
    }

    void FollowMouse()
    {
        rootRect.anchoredPosition = Vector2.zero;
    }


    public void Show(TotemData data)
    {
        if (data == null) return;

        root.SetActive(true);

        iconImage.sprite = data.icon;
        nameText.text = data.totemName;
        descriptionText.text = data.description;
        buffText.text = GetBuffText(data);
        comparisonText.text = GetComparison(data);

        ApplyRarityVisual(data);

        StopAllCoroutines();
        StartCoroutine(FadeInScale());
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    void HideInstant()
    {
        root.SetActive(false);
        canvasGroup.alpha = 0f;
        rootRect.localScale = Vector3.one * 0.95f;
    }

    void ApplyRarityVisual(TotemData data)
    {
        Color c = Color.white;

        switch (data.rarity)
        {
            case TotemRarity.Common:
                c = Color.white;
                break;
            case TotemRarity.Rare:
                c = new Color(0.3f, 0.7f, 1f);
                break;
            case TotemRarity.Legendary:
                c = new Color(1f, 0.6f, 0.1f);
                break;
        }

        nameText.color = c;
        if (glow != null) glow.color = c;
    }

    IEnumerator FadeInScale()
    {
        float t = 0f;

        canvasGroup.alpha = 0f;
        rootRect.localScale = Vector3.one * 0.9f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 10f;
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = ease;
            rootRect.localScale = Vector3.Lerp(
                Vector3.one * 0.9f,
                Vector3.one,
                ease
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rootRect.localScale = Vector3.one;
    }

    IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 10f;

            canvasGroup.alpha = 1f - t;
            rootRect.localScale = Vector3.Lerp(
                Vector3.one,
                Vector3.one * 0.95f,
                t
            );

            yield return null;
        }

        root.SetActive(false);
    }

    string GetBuffText(TotemData data)
    {
        switch (data.totemType)
        {
            case TotemType.Power: return "+30% Damage";
            case TotemType.Swiftness: return "+20% Move Speed";
            case TotemType.RapidBullets: return "+30% Fire Rate";
            case TotemType.TripleShot: return "+2 Projectiles";
            case TotemType.DualFire: return "+1 Projectile";
            case TotemType.Piercing: return "+1 Pierce";
            case TotemType.Ricochet: return "+1 Ricochet";
            case TotemType.Recovery: return "+1.5 HP/sec";
            case TotemType.Shielding: return "+25 Shield";
            case TotemType.SecondWind: return "Revive once";
            case TotemType.Dash: return "Unlock Dash";
            case TotemType.BloodPrice: return "Damage scales with missing HP";
            case TotemType.Retaliation: return "Damage nearby enemies";
        }

        return "";
    }

    string GetComparison(TotemData data)
    {
        if (stats == null) return "";

        switch (data.totemType)
        {
            case TotemType.Power:
                return Compare(stats.Damage, stats.Damage * 1.3f, "Damage");

            case TotemType.Swiftness:
                return Compare(stats.MoveSpeed, stats.MoveSpeed * 1.2f, "Speed");

            case TotemType.Piercing:
                return $"Pierce: {stats.Pierce} → <color=#00FF88>{stats.Pierce + 1}</color>";

            case TotemType.Ricochet:
                return $"Ricochet: {stats.Ricochet} → <color=#00FF88>{stats.Ricochet + 1}</color>";

            case TotemType.BloodPrice:
                if (hero == null) return "";

                float missing = 1f - (hero.CurrentHealth / hero.MaxHealth);
                float newDamage = stats.Damage * (1f + missing * 0.8f);

                return Compare(stats.Damage, newDamage, "Damage (Low HP)");
        }

        return "";
    }

    string Compare(float current, float after, string label)
    {
        string color = after > current ? "#00FF88" : "#FF5555";

        return $"{label}\nCurrent: {current:F2}\nAfter: <color={color}>{after:F2}</color>";
    }

    void Start()
    {
        Invoke(nameof(DebugForceShow), 1f);
    }

    void DebugForceShow()
    {
        TotemData any = FindFirstObjectByType<TotemSelectionUI>()?.allTotems[0];
        Show(any);
    }

}
