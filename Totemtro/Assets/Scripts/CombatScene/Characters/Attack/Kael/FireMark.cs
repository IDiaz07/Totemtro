using UnityEngine;

public class FireMark : MonoBehaviour
{
    public float duration = 2.5f;
    public float fadeSpeed = 2f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        duration -= Time.deltaTime;

        if (duration <= 0f)
        {
            Color c = sr.color;
            c.a -= fadeSpeed * Time.deltaTime;
            sr.color = c;

            if (c.a <= 0f)
                Destroy(gameObject);
        }
    }
}