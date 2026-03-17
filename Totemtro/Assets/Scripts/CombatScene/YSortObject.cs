using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortObject : MonoBehaviour
{
    public Transform sortPoint;
    public int sortingOffset = 10000; // más grande
    public int precision = 10;        // menor precisión

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sortPoint == null)
            sortPoint = transform;
    }

    void LateUpdate()
    {
        sr.sortingOrder =
            sortingOffset - Mathf.RoundToInt(sortPoint.position.y * precision);
    }
}