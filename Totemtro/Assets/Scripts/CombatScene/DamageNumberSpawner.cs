using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    public GameObject damageNumberPrefab;

    public void SpawnDamage(float amount, bool isCritical)
    {
        if (damageNumberPrefab == null) return;

        // Offset base arriba
        Vector3 baseOffset = Vector3.up * 0.8f;

        // Variación aleatoria
        float randomX = Random.Range(-0.4f, 0.4f);
        float randomY = Random.Range(0f, 0.3f);

        Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

        Vector3 spawnPos = transform.position + baseOffset + randomOffset;

        // 🔥 Rotación basada en posición horizontal
        float maxRotation = 15f; // máximo grados
        float rotationZ = (randomX / 0.4f) * maxRotation;
        rotationZ += Random.Range(-3f, 3f);


        Quaternion rotation = Quaternion.Euler(0f, 0f, rotationZ);

        GameObject obj = Instantiate(
            damageNumberPrefab,
            spawnPos,
            rotation
        );

        DamageNumber dmg = obj.GetComponent<DamageNumber>();

        if (dmg != null)
            dmg.SetDamage(amount, isCritical);
    }

}
