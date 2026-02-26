using UnityEngine;

public class SummonerDirectionalSprite : MonoBehaviour
{
    [Header("Walk Sprites")]
    public Sprite walkBackLeft;
    public Sprite walkBackRight;
    public Sprite walkBack;
    public Sprite walkFrontLeft;
    public Sprite walkFrontRight;
    public Sprite walkFront;
    public Sprite walkLeft;
    public Sprite walkRight;

    [Header("Summon Sprites")]
    public Sprite summonBackLeft;
    public Sprite summonBackRight;
    public Sprite summonBack;
    public Sprite summonFrontLeft;
    public Sprite summonFrontRight;
    public Sprite summonFront;
    public Sprite summonLeft;
    public Sprite summonRight;

    Transform player;
    SpriteRenderer sr;

    bool isSummoning = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetSummoning(bool value)
    {
        isSummoning = value;
    }

    void Update()
    {
        if (player == null || sr == null) return;

        Vector2 dir =
            (player.position - transform.parent.position).normalized;

        float angle =
            Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360f;

        int sector = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;

        SetSpriteBySector(sector);
    }

    void SetSpriteBySector(int sector)
    {
        if (!isSummoning)
        {
            switch (sector)
            {
                case 0: sr.sprite = walkRight; break;
                case 1: sr.sprite = walkBackRight; break;
                case 2: sr.sprite = walkBack; break;
                case 3: sr.sprite = walkBackLeft; break;
                case 4: sr.sprite = walkLeft; break;
                case 5: sr.sprite = walkFrontLeft; break;
                case 6: sr.sprite = walkFront; break;
                case 7: sr.sprite = walkFrontRight; break;
            }
        }
        else
        {
            switch (sector)
            {
                case 0: sr.sprite = summonRight; break;
                case 1: sr.sprite = summonBackRight; break;
                case 2: sr.sprite = summonBack; break;
                case 3: sr.sprite = summonBackLeft; break;
                case 4: sr.sprite = summonLeft; break;
                case 5: sr.sprite = summonFrontLeft; break;
                case 6: sr.sprite = summonFront; break;
                case 7: sr.sprite = summonFrontRight; break;
            }
        }
    }
}
