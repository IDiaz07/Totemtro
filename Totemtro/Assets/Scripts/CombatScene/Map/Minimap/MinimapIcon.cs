using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    public Transform Target;
    public Image image;

    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (image == null)
            image = GetComponent<Image>();
    }

    public void UpdatePosition(Vector2 pos)
    {
        if (rect == null) return;

        rect.anchoredPosition = pos;
    }

    public void SetVisible(bool state)
    {
        if (image != null)
            image.enabled = state;
    }
}