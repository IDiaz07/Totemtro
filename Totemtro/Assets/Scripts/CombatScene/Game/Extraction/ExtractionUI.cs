using UnityEngine;
using UnityEngine.UI;

public class ExtractionUI : MonoBehaviour
{
    public static ExtractionUI Instance;

    public GameObject root;
    public Image fillBar;

    float totalTime;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show(float duration)
    {
        totalTime = duration;
        root.SetActive(true);
    }

    public void UpdateBar(float current)
    {
        fillBar.fillAmount = current / totalTime;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}