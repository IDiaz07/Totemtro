using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 30f;
    public float contactDamage = 10f;

    float currentHealth;

    Transform player;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Aquí luego metemos sistema de daño real
            Debug.Log("Player hit");
        }
    }
}
