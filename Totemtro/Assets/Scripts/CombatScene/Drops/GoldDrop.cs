using UnityEngine;

public class GoldDrop : MonoBehaviour
{
    public int goldValue = 5;

    [Header("Spawn Physics")]
    public float settleTime = 0.45f;

    [Header("Idle Rotate")]
    public float rotateSpeed = 120f;

    [Header("Magnet")]
    public float magnetRange = 2.5f;
    public float magnetAcceleration = 22f;
    public float maxMagnetSpeed = 11f;

    Transform player;
    Rigidbody2D rb;

    float spawnTimer;
    float currentSpeed;

    bool settled = false;
    bool magnetized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (!settled)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= settleTime)
            {
                settled = true;

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= magnetRange)
            magnetized = true;

        if (magnetized)
            MagnetMovement();
        else
            IdleRotate();
    }

    void IdleRotate()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    void MagnetMovement()
    {
        Vector2 dir =
            (player.position - transform.position).normalized;

        currentSpeed += magnetAcceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxMagnetSpeed);

        transform.position +=
            (Vector3)(dir * currentSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GoldSystem gold =
            other.GetComponent<GoldSystem>();

        if (gold != null)
            gold.AddGold(goldValue);

        Destroy(gameObject);
    }
}
