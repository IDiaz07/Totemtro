using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineUI : MonoBehaviour
{
    public Image[] reels;
    public List<SlotIconData> icons;

    public GameObject rewardPopup;
    public Image rewardIcon;

    private Dictionary<SlotIconType, Sprite> iconDict;

    void Awake()
    {
        iconDict = new Dictionary<SlotIconType, Sprite>();

        foreach (var icon in icons)
        {
            iconDict[icon.type] = icon.sprite;
        }
    }

    public void StartSpin(System.Action onComplete)
    {
        StartCoroutine(SpinRoutine(onComplete));
    }

    IEnumerator SpinRoutine(System.Action onComplete)
    {
        float duration = 1.5f;
        float timer = 0f;

        while (timer < duration)
        {
            foreach (var reel in reels)
            {
                reel.sprite = icons[Random.Range(0, icons.Count)].sprite;
            }

            timer += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        onComplete?.Invoke();
    }

    public void SetFinalResult(SlotIconType result)
    {
        Sprite sprite = iconDict[result];

        foreach (var reel in reels)
        {
            reel.sprite = sprite;
        }
    }

    public void ShowReward(Sprite icon)
    {
        rewardPopup.SetActive(true);

        if (icon != null)
            rewardIcon.sprite = icon;
    }
}