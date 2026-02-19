using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MaterialDrop : MonoBehaviour
{
    [Header("Data")]
    public ItemData item;
    public int amount = 1;

    [Header("Attraction")]
    public float attractRadius = 2.5f;
    public float attractSpeed = 12f;
    public float attractAcceleration = 20f;

    [Header("Spawn Animation")]
    public float bounceForce = 2f;
    public float spawnScaleDuration = 0.15f;

    [Header("Hover")]
    public float hoverAmplitude = 0.08f;
    public float hoverSpeed = 3f;

    [Header("Pickup FX")]
    public float collectDuration = 0.15f;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Transform player;

    bool isAttracting = false;
    bool isCollected = false;
    float currentAttractSpeed;

    Vector3 hoverOffset;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    public void Initialize(ItemData data, int amountToGive)
    {
        item = data;
        amount = amountToGive;

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        sr.sprite = item.icon;
        sr.sortingOrder = 5;

        transform.localScale = Vector3.zero;
        StartCoroutine(SpawnAnimation());

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        rb.AddForce(randomDir * bounceForce, ForceMode2D.Impulse);

        currentAttractSpeed = attractSpeed;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isCollected) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attractRadius)
            isAttracting = true;

        if (isAttracting)
        {
            rb.simulated = false; // 🔥 Desactiva física al atraer

            currentAttractSpeed += attractAcceleration * Time.deltaTime;

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                currentAttractSpeed * Time.deltaTime
            );

            if (distance < 0.3f)
                TryCollect();
        }
        else
        {
            Hover();
        }
    }

    // ==========================================
    // SPAWN FX
    // ==========================================

    IEnumerator SpawnAnimation()
    {
        float t = 0f;

        while (t < spawnScaleDuration)
        {
            t += Time.deltaTime;
            float ease = 1f - Mathf.Pow(1f - t / spawnScaleDuration, 3f);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ease);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    void Hover()
    {
        hoverOffset.y = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position += hoverOffset * Time.deltaTime;
    }

    // ==========================================
    // COLLECTION
    // ==========================================

    void TryCollect()
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        bool added = inventory.AddItem(item, amount);

        if (!added)
        {
            isAttracting = false;
            currentAttractSpeed = attractSpeed;
            return;
        }

        StartCoroutine(CollectAnimation());
    }

    IEnumerator CollectAnimation()
    {
        isCollected = true;

        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < collectDuration)
        {
            t += Time.deltaTime;
            float lerp = t / collectDuration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, lerp);
            sr.color = new Color(1, 1, 1, 1 - lerp);

            yield return null;
        }

        Destroy(gameObject);
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }
}
