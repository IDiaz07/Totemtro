using UnityEngine;

public class BodyBreathing : MonoBehaviour
{
    public float scaleAmount = 0.02f;
    public float speed = 1.5f;

    Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        float s = 1f + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = startScale * s;
    }
}
