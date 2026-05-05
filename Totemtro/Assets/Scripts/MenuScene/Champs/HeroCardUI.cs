using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HeroCardUI : MonoBehaviour
{
    public Image portrait;
    public Image portraitBack;

    public TMP_Text nameText;

    public GameObject lockOverlay;
    public GameObject selectHighlight;

    [Header("Level UI")]
    public GameObject lvlBackground;
    public TMP_Text lvlText;

    [Header("Unlock UI")]
    public TMP_Text gemCostText;
    public Button unlockButton;

    HeroData heroData;

    void Update()
    {
        if (heroData == null || HeroProgressSystem.Instance == null)
            return;

        bool unlocked = HeroProgressSystem.Instance.IsUnlocked(heroData.heroType);

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (unlockButton != null)
            unlockButton.gameObject.SetActive(!unlocked);

        if (gemCostText != null)
            gemCostText.gameObject.SetActive(!unlocked);
    }

    public void Setup(HeroData data, bool isSelected)
    {
        heroData = data;

        if (portrait != null)
            portrait.sprite = data.ChampsHeadIcon;

        if (portraitBack != null)
            portraitBack.sprite = data.ChampsHeadIcon;

        if (nameText != null)
            nameText.text = data.heroName;

        if (HeroProgressSystem.Instance == null)
        {
            Debug.LogError("HeroProgressSystem no existe en escena.");
            return;
        }

        bool unlocked =
            HeroProgressSystem.Instance.IsUnlocked(data.heroType);

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (selectHighlight != null)
            selectHighlight.SetActive(isSelected);

        if (lvlBackground != null)
            lvlBackground.SetActive(unlocked);

        if (lvlText != null)
            lvlText.gameObject.SetActive(unlocked);

        if (unlocked && lvlText != null)
        {
            int level =
                HeroProgressSystem.Instance.GetLevel(data.heroType);

            lvlText.text = level.ToString();

            if (lvlBackground != null)
            {
                Image bgImage = lvlBackground.GetComponent<Image>();

                if (bgImage != null)
                {
                    if (HeroProgressSystem.Instance.IsMaxLevel(data.heroType))
                    {
                        bgImage.color = Color.red;
                    }
                    else
                    {
                        Color cyan;
                        ColorUtility.TryParseHtmlString("#00FFF3", out cyan);
                        bgImage.color = cyan;
                    }
                }
            }
        }

        if (unlockButton != null)
            unlockButton.gameObject.SetActive(!unlocked);

        if (gemCostText != null)
        {
            gemCostText.gameObject.SetActive(!unlocked);
            gemCostText.text = data.gemCost.ToString();
        }
    }

    public void OnClick()
    {
        if (!HeroProgressSystem.Instance.IsUnlocked(heroData.heroType))
            return;

        HeroSelectionSystem.Instance.SelectHero(heroData);
    }

    public void OnUnlockClicked()
    {
        bool success =
            HeroProgressSystem.Instance.TryUnlock(heroData);

        if (success)
            StartCoroutine(UnlockAnimation());
    }

    IEnumerator UnlockAnimation()
    {
        float t = 0f;
        float duration = 0.4f;

        Vector3 originalScale = transform.localScale;

        while (t < duration)
        {
            float scale =
                Mathf.Lerp(1f, 1.15f, t / duration);

            transform.localScale =
                originalScale * scale;

            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        Setup(heroData, false);
    }

    public void OpenDetails()
    {
        ChampDetailPanelUI.Instance.Open(heroData);
    }

    public void PlayFragmentImpact()
    {
        StartCoroutine(Punch());
    }

    IEnumerator Punch()
    {
        Vector3 original = transform.localScale;
        float t = 0;
        float duration = 0.15f;

        while (t < duration)
        {
            float scale = Mathf.Lerp(1f, 1.1f, t / duration);
            transform.localScale = original * scale;
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = original;
    }
}