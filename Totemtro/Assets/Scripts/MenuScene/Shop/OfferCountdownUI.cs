using UnityEngine;
using TMPro;

public class OfferCountdownUI : MonoBehaviour
{
    public TMP_Text timerText;

    void Update()
    {
        if (LimitedHeroOfferSystem.Instance == null)
            return;

        var time =
            LimitedHeroOfferSystem.Instance.GetRemainingTime();

        if (time.TotalSeconds <= 0)
        {
            timerText.text = "Expired";
            return;
        }

        timerText.text =
            $"{time.Days}d {time.Hours}h {time.Minutes}m";
    }
}