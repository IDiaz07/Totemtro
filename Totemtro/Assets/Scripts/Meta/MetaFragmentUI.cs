using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MetaFragmentUI : MonoBehaviour
{
    public static MetaFragmentUI Instance;

    [Header("UI")]
    public GameObject panel;
    public Image heroHeadIcon;
    public TMP_Text addedText;
    public TMP_Text totalText;
    public Slider progressBar;

    [Header("Config")]
    public float visibleTime = 2f;
    public float barAnimationTime = 0.6f;

    void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    void OnEnable()
    {
        HeroProgressSystem.OnFragmentsAdded += Show;
    }

    void OnDisable()
    {
        HeroProgressSystem.OnFragmentsAdded -= Show;
    }

    void Show(HeroType type, int addedAmount)
    {
        HeroData hero = HeroProgressSystem.Instance.GetHeroData(type);
        if (hero == null)
            return;

        panel.SetActive(true);

        heroHeadIcon.sprite = hero.ChampsHeadIcon;
        addedText.text = "+" + addedAmount;

        UpdateProgress(type);

        CancelInvoke();
        Invoke(nameof(Hide), visibleTime);
    }

    void UpdateProgress(HeroType type)
    {
        bool unlocked = HeroProgressSystem.Instance.IsUnlocked(type);
        int level = HeroProgressSystem.Instance.GetLevel(type);

        int requiredFragments;

        if (!unlocked)
        {
            requiredFragments =
                HeroProgressSystem.Instance.GetRequiredFragmentsForUnlock(type);
        }
        else
        {
            requiredFragments = 10 * level; // upgrade rule real
        }

        int current = HeroProgressSystem.Instance.GetFragments(type);
        int clampedCurrent = Mathf.Clamp(current, 0, requiredFragments);

        progressBar.maxValue = requiredFragments;

        totalText.text = clampedCurrent + " / " + requiredFragments;

        StopAllCoroutines();
        StartCoroutine(AnimateBar(clampedCurrent));
    }

    IEnumerator AnimateBar(int targetValue)
    {
        float startValue = progressBar.value;
        float time = 0f;

        while (time < barAnimationTime)
        {
            time += Time.deltaTime;
            float t = time / barAnimationTime;

            progressBar.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        progressBar.value = targetValue;
    }

    void Hide()
    {
        panel.SetActive(false);
    }
}