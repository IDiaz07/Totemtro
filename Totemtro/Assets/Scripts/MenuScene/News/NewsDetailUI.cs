using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewsDetailUI : MonoBehaviour
{
    public static NewsDetailUI Instance;

    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text date;
    [SerializeField] private TMP_Text content;
    [SerializeField] private Image image;

    private void Awake()
    {
        Instance = this;

        if (container != null)
            container.SetActive(false);
    }

    public void Show(NewsData data)
    {
        if (container == null)
            return;

        container.SetActive(true);

        title.text = data.title;
        date.text = data.date;
        content.text = data.content;

        if (image != null && data.image != null)
        {
            image.sprite = data.image;
            image.gameObject.SetActive(true);
        }
        else if (image != null)
        {
            image.gameObject.SetActive(false);
        }
    }

    public void Close()
    {
        if (container != null)
            container.SetActive(false);
    }
}