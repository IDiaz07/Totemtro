using UnityEngine;
using UnityEngine.UI;

public class CarouselEffect : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scaleMultiplier = 0.2f;

    void Update()
    {
        foreach (Transform child in scrollRect.content)
        {
            float distance = Mathf.Abs(
                scrollRect.viewport.position.x - child.position.x);

            float scale = 1 - Mathf.Clamp01(distance / 800f) * scaleMultiplier;

            child.localScale = Vector3.Lerp(
                child.localScale,
                new Vector3(scale, scale, 1),
                Time.deltaTime * 10f);
        }
    }
}