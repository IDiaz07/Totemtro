using UnityEngine;

public class UIGlowShake : MonoBehaviour
{
    public float intensity = 3f;   // fuerza de vibración
    public float speed = 80f;      // velocidad

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * intensity;
        float y = Mathf.Cos(Time.time * speed * 1.3f) * intensity;

        transform.localPosition = startPos + new Vector3(x, y, 0);
    }
}