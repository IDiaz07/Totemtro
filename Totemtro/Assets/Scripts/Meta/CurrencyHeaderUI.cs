using UnityEngine;
using TMPro;

public class CurrencyHeaderUI : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text gemsText;

    void OnEnable()
    {
        MetaCurrencySystem.OnCurrencyChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        MetaCurrencySystem.OnCurrencyChanged -= Refresh;
    }

    void Refresh()
    {
        if (MetaCurrencySystem.Instance == null)
            return;

        goldText.text = MetaCurrencySystem.Instance.Gold.ToString("N0");
        gemsText.text = MetaCurrencySystem.Instance.Gems.ToString("N0");
    }
}