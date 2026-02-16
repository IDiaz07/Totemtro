using UnityEngine;

public class XPDrop : MonoBehaviour
{
    public float xpValue = 10f;

    [Header("Spawn Physics")]
    public float settleTime = 0.35f;

    [Header("Idle Float")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.12f;

    [Header("Breathing")]
    public float breatheSpeed = 2.5f;
    public float breatheAmount = 0.07f;

    [Header("Magnet")]
    public float magnetRange = 3f;
    public float magnetAcceleration = 30f;
    public float maxMagnetSpeed = 14f;

    Transform player;
    Rigidbody2D rb;

    Vector3 startPos;
    Vector3 baseScale;

    float randomOffset;
    float currentSpeed;
    float spawnTimer;

    bool settled = false;
    bool magnetized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        baseScale = transform.localScale;
        randomOffset = Random.Range(0f, 100f);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Esperar a que termine el impulso inicial
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

                startPos = transform.position;
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= magnetRange)
            magnetized = true;

        if (magnetized)
            MagnetMovement();
        else
        {
            FloatEffect();
            BreatheEffect();
        }
    }

    void FloatEffect()
    {
        float newY =
            Mathf.Sin((Time.time + randomOffset) * floatSpeed)
            * floatHeight;

        transform.position =
            startPos + new Vector3(0f, newY, 0f);
    }

    void BreatheEffect()
    {
        float scale =
            1f + Mathf.Sin((Time.time + randomOffset) * breatheSpeed)
            * breatheAmount;

        transform.localScale = baseScale * scale;
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

        PlayerExperience xp =
            other.GetComponent<PlayerExperience>();

        if (xp != null)
            xp.AddXP(xpValue);

        Destroy(gameObject);
    }
}
