using UnityEngine;
using System.Collections;

public class PortalRay : MonoBehaviour
{
    public float growSpeed = 6f;
    public float fadeSpeed = 2f;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(0.1f, 0f, 1f);

        StartCoroutine(AnimateRay());
    }

    IEnumerator AnimateRay()
    {
        float t = 0f;

        while (t < 1f)
        {
            transform.localScale =
                new Vector3(0.15f, t * growSpeed, 1f);

            sr.color =
                new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t);

            t += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        Destroy(gameObject);
    }
}
