using UnityEngine;

public class FangInfinityMotion : MonoBehaviour
{
    public float width = 1.5f;
    public float height = 0.8f;
    public float speed = 2f;

    Vector3 center;

    void Start()
    {
        center = transform.position;
    }

    void Update()
    {
        float t = Time.time * speed;

        float x = Mathf.Sin(t) * width;
        float y = Mathf.Sin(t * 2f) * height;

        transform.position = center + new Vector3(x, y, 0f);
    }
}