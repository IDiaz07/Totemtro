using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount = 5;
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

        GoldSystem gold = FindFirstObjectByType<GoldSystem>();
        gold.AddGold(amount);

        Destroy(gameObject);
    }
}
