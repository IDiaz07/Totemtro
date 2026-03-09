using UnityEngine;

public class AfterImage : MonoBehaviour
{
    SpriteRenderer sr;

    public float lifeTime = 0.35f;
    public float fadeSpeed = 4f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(SpriteRenderer original)
    {
        if (original == null) return;

        sr.sprite = original.sprite;
        sr.flipX = original.flipX;
        sr.flipY = original.flipY;

        sr.sortingLayerID = original.sortingLayerID;
        sr.sortingOrder = original.sortingOrder - 1;

        transform.localScale = original.transform.lossyScale;
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;

        Color c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        if (lifeTime <= 0f)
            Destroy(gameObject);
    }
}