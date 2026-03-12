using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelenePool : MonoBehaviour
{
    public float duration = 4f;
    public float tickRate = 0.4f;

    public float slowAmount = 0.5f;
    public float healAmount = 2f;
    public float speedBuff = 1.3f;

    public float damage = 5f;

    // ⭐ prefab del número de curación
    public GameObject healNumberPrefab;

    HashSet<Enemy> enemiesInside = new HashSet<Enemy>();
    HashSet<PlayerStats> playersInside = new HashSet<PlayerStats>();

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemiesInside.Add(enemy);
            StartCoroutine(DamageEnemy(enemy));
            return;
        }

        PlayerStats player = other.GetComponent<PlayerStats>();

        if (player != null)
        {
            playersInside.Add(player);
            StartCoroutine(BuffPlayer(player));
        }
    }

    IEnumerator DamageEnemy(Enemy enemy)
    {
        while (enemiesInside.Contains(enemy))
        {
            enemy.TakeDamage(damage, Vector2.zero, false);
            yield return new WaitForSeconds(tickRate);
        }
    }

    IEnumerator BuffPlayer(PlayerStats player)
    {
        player.SetSpeedMultiplier(speedBuff);

        while (playersInside.Contains(player))
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                hp.Heal(healAmount);

                // ⭐ spawn heal number
                SpawnHealNumber(player.transform.position, healAmount);
            }

            yield return new WaitForSeconds(tickRate);
        }

        player.SetSpeedMultiplier(1f);
    }

    void SpawnHealNumber(Vector3 pos, float amount)
    {
        if (healNumberPrefab == null) return;

        GameObject obj = Instantiate(
            healNumberPrefab,
            pos + Vector3.up * 0.6f,
            Quaternion.identity
        );

        HealNumber heal = obj.GetComponent<HealNumber>();

        if (heal != null)
            heal.SetHeal(amount);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null) enemiesInside.Remove(enemy);

        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null) playersInside.Remove(player);
    }
}