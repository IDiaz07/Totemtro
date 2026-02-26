using UnityEngine;

public class ExtractionArrowUI : MonoBehaviour
{
    public static ExtractionArrowUI Instance;

    public RectTransform arrow;
    Transform target;

    void Awake()
    {
        Instance = this;
        arrow.gameObject.SetActive(false);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        arrow.gameObject.SetActive(true);
    }

    void Update()
    {
        if (target == null)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        Vector3 dir = target.position -
                      GameObject.FindGameObjectWithTag("Player").transform.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrow.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}