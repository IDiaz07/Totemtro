using System.Collections;
using UnityEngine;

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

    Vector3 basePosition;

    Transform outlineTransform;
    float glowPulse;

    [Header("Pickup Protection")]
    public float pickupDelay = 2f;

    bool canBePickedUp = false;

    // ==========================================
    // INITIALIZATION
    // ==========================================

    public void Initialize(ItemData data, int amountToGive, bool applyPickupDelay)
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

        if (applyPickupDelay)
            StartCoroutine(PickupDelayRoutine());
        else
            canBePickedUp = true;

        StartCoroutine(ThrowStretch());
        CreateGoldenOutline();
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        if (isCollected)
            return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distance =
            Vector2.Distance(transform.position, player.position);

        if (canBePickedUp && distance <= attractRadius && CanPlayerPickUp())
            isAttracting = true;

        if (isAttracting)
        {
            rb.simulated = false;

            currentAttractSpeed +=
                attractAcceleration * Time.deltaTime;

            transform.position =
                Vector2.MoveTowards(
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

        AnimateOutline();
    }

    // ==========================================
    // COLLECTION
    // ==========================================

    void TryCollect()
    {
        if (!canBePickedUp)
            return;

        MetaInventory meta = MetaInventory.Instance;

        if (meta == null)
            return;

        int remaining = amount;

        // BAG FIRST
        for (int i = 0; i < meta.bagSlots.Length && remaining > 0; i++)
        {
            var slot = meta.bagSlots[i];

            if (slot.IsEmpty())
            {
                int toAdd = Mathf.Min(item.maxStack, remaining);

                slot.item = item;
                slot.amount = toAdd;

                remaining -= toAdd;
            }
            else if (slot.item == item && slot.amount < item.maxStack)
            {
                int space = item.maxStack - slot.amount;
                int toAdd = Mathf.Min(space, remaining);

                slot.amount += toAdd;
                remaining -= toAdd;
            }
        }

        // SUCCESS
        if (remaining <= 0)
        {
            meta.NotifyInventoryChanged();
            meta.SaveMetaInventory();

            StartCoroutine(CollectAnimation());
            return;
        }

        // NO SPACE → DROP STAYS
        isAttracting = false;
        currentAttractSpeed = attractSpeed;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        basePosition = transform.position;
    }

    bool CanPlayerPickUp()
    {
        MetaInventory meta = MetaInventory.Instance;

        if (meta == null)
            return false;

        foreach (var slot in meta.bagSlots)
        {
            if (slot.IsEmpty())
                return true;

            if (slot.item == item &&
                slot.amount < item.maxStack)
                return true;
        }

        return false;
    }

    // ==========================================
    // FX
    // ==========================================

    IEnumerator CollectAnimation()
    {
        isCollected = true;

        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < collectDuration)
        {
            t += Time.deltaTime;
            float lerp = t / collectDuration;

            transform.localScale =
                Vector3.Lerp(startScale, Vector3.zero, lerp);

            sr.color =
                new Color(1, 1, 1, 1 - lerp);

            yield return null;
        }

        Destroy(gameObject);
    }

    void FindPlayer()
    {
        GameObject p =
            GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
    }

    IEnumerator SpawnAnimation()
    {
        float t = 0f;

        while (t < spawnScaleDuration)
        {
            t += Time.deltaTime;

            float ease =
                1f - Mathf.Pow(1f - t / spawnScaleDuration, 3f);

            transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, ease);

            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    void Hover()
    {
        float yOffset =
            Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;

        transform.position =
            new Vector3(
                basePosition.x,
                basePosition.y + yOffset,
                basePosition.z
            );
    }

    void CreateGoldenOutline()
    {
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localScale = Vector3.one * 1.15f;

        SpriteRenderer outlineSR =
            outlineObj.AddComponent<SpriteRenderer>();

        outlineSR.sprite = sr.sprite;
        outlineSR.sortingOrder = sr.sortingOrder - 1;
        outlineSR.color = new Color(1f, 0.84f, 0.2f, 0.9f);

        outlineTransform = outlineObj.transform;
    }

    void AnimateOutline()
    {
        if (outlineTransform == null)
            return;

        glowPulse =
            Mathf.Sin(Time.time * 4f) * 0.05f;

        outlineTransform.localScale =
            Vector3.one * (1.15f + glowPulse);
    }

    IEnumerator ThrowStretch()
    {
        Vector3 original = transform.localScale;

        transform.localScale =
            new Vector3(original.x * 1.2f, original.y * 0.8f, original.z);

        yield return new WaitForSeconds(0.08f);

        transform.localScale = original;
    }

    IEnumerator PickupDelayRoutine()
    {
        canBePickedUp = false;

        yield return new WaitForSeconds(pickupDelay);

        canBePickedUp = true;
    }
}