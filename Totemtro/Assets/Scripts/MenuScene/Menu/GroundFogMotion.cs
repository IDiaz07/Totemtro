using UnityEngine;

public class GroundFogMotion : MonoBehaviour
{
    public float speed = 0.2f;
    public float amplitude = 0.3f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}