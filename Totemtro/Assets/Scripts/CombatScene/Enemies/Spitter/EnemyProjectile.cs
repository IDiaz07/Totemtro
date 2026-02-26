using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float damage = 10f;
    public float lifeTime = 5f;

    Vector2 direction;

    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null)
            player.TakeDamage(damage, direction);

        Destroy(gameObject);
    }
}
