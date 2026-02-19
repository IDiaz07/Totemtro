using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 2f;
    public float radius = 2f;
    public float damage = 25f;
    public float knockbackForce = 5f;

    public float critChance = 0.1f; // 10% por defecto

    void Start()
    {
        Invoke(nameof(Explode), delay);
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                Vector2 dir = (enemy.transform.position - transform.position).normalized;

                bool isCritical = Random.value <= critChance;

                enemy.TakeDamage(damage, dir * knockbackForce, isCritical);
            }
        }

        Destroy(gameObject);
    }
}
