using UnityEngine;
using UnityEngine.UI;

public class NewsPanelUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject newsItemPrefab;
    [SerializeField] private ScrollRect scrollRect;

    public void Open()
    {
        if (!IsValid())
            return;

        Populate();
        ScrollToTop();
    }

    private bool IsValid()
    {
        return contentParent != null &&
               newsItemPrefab != null &&
               scrollRect != null &&
               NewsManager.Instance != null;
    }

    private void Populate()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var newsList = NewsManager.Instance.GetSortedNews();

        if (newsList == null || newsList.Count == 0)
            return;

        foreach (var news in newsList)
        {
            GameObject obj = Instantiate(newsItemPrefab, contentParent);

            var item = obj.GetComponent<NewsCardUI>();
            if (item == null)
                continue;

            item.Setup(news);
        }
    }

    private void ScrollToTop()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}