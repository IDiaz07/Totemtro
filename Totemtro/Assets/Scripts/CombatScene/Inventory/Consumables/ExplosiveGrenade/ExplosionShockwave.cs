using UnityEngine;

public class ExplosionShockwave : MonoBehaviour
{
    public float maxScale = 3f;
    public float duration = 0.3f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;

        transform.localScale = Vector3.one * Mathf.Lerp(0f, maxScale, t);

        Color c = GetComponent<SpriteRenderer>().color;
        c.a = 1f - t;
        GetComponent<SpriteRenderer>().color = c;

        if (t >= 1f)
            Destroy(gameObject);
    }
}