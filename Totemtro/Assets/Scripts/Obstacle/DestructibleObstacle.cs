using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    public float health = 20f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}