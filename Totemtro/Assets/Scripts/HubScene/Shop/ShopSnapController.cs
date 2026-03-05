using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopSnapController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public List<RectTransform> sections;

    public float snapSpeed = 10f;

    int targetIndex;
    bool snapping;

    void Update()
    {
        if (!snapping) return;

        float targetX = -sections[targetIndex].anchoredPosition.x;
        float newX = Mathf.Lerp(
            content.anchoredPosition.x,
            targetX,
            Time.deltaTime * snapSpeed
        );

        content.anchoredPosition =
            new Vector2(newX, content.anchoredPosition.y);

        if (Mathf.Abs(newX - targetX) < 2f)
        {
            content.anchoredPosition =
                new Vector2(targetX, content.anchoredPosition.y);
            snapping = false;
        }
    }

    public void SnapTo(int index)
    {
        targetIndex = Mathf.Clamp(index, 0, sections.Count - 1);
        snapping = true;
    }
}