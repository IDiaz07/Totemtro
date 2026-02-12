using UnityEngine;

public class BodyBobbing : MonoBehaviour
{
    public float amplitude = 0.05f;   // cuánto se mueve
    public float frequency = 2.5f;    // velocidad

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + Vector3.up * offset;
    }
}
