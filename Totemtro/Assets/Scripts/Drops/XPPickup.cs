using UnityEngine;

public class XPPickup : MonoBehaviour
{
    public int xpAmount = 5;
    public float magnetSpeed = 6f;

    Transform player;

    void Start()
    {
        player = FindFirstObjectByType<HeroController>().transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < 3f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                magnetSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerExperience xp = FindFirstObjectByType<PlayerExperience>();
        xp.AddXP(xpAmount);

        Destroy(gameObject);
    }
}
