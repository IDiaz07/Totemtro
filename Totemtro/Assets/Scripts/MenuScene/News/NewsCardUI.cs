using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewsCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text date;
    [SerializeField] private TMP_Text preview;
    [SerializeField] private GameObject newBadge;
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    private NewsData data;

    public void Setup(NewsData news)
    {
        data = news;

        title.text = news.title;
        date.text = news.date;

        preview.text = news.content.Substring(0, Mathf.Min(100, news.content.Length)) + "...";

        bool isNew = !NewsManager.Instance.IsSeen(news.id);
        newBadge.SetActive(isNew);

        if (image != null && news.image != null)
        {
            image.sprite = news.image;
            image.gameObject.SetActive(true);
        }
        else if (image != null)
        {
            image.gameObject.SetActive(false);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenDetail);
    }

    private void OpenDetail()
    {
        NewsDetailUI.Instance.Show(data);

        if (!NewsManager.Instance.IsSeen(data.id))
            NewsManager.Instance.MarkAsSeen(data.id);
    }
}