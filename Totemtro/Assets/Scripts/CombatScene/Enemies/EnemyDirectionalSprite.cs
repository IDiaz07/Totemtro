using UnityEngine;

public class EnemyDirectionalSprite : MonoBehaviour
{
    [Header("Directional Sprites (8)")]
    public Sprite backLeft;
    public Sprite backRight;
    public Sprite back;
    public Sprite frontLeft;
    public Sprite frontRight;
    public Sprite front;
    public Sprite left;
    public Sprite right;

    Transform player;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || spriteRenderer == null) return;

        Vector2 dir =
            (player.position - transform.position).normalized;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        SetSprite(angle);
    }

    void SetSprite(float angle)
    {
        // Normaliza a 0-360
        if (angle < 0) angle += 360f;

        if (angle >= 337.5f || angle < 22.5f)
            spriteRenderer.sprite = right;

        else if (angle < 67.5f)
            spriteRenderer.sprite = backRight;

        else if (angle < 112.5f)
            spriteRenderer.sprite = back;

        else if (angle < 157.5f)
            spriteRenderer.sprite = backLeft;

        else if (angle < 202.5f)
            spriteRenderer.sprite = left;

        else if (angle < 247.5f)
            spriteRenderer.sprite = frontLeft;

        else if (angle < 292.5f)
            spriteRenderer.sprite = front;

        else
            spriteRenderer.sprite = frontRight;
    }
}
