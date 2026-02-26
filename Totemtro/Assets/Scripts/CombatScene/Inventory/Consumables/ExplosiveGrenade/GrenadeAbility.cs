using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Grenade")]
public class GrenadeAbility : ActiveAbilityBase
{
    public GameObject grenadePrefab;
    public float throwForce = 6f;

    protected override bool Activate(GameObject user)
    {
        Debug.Log("Activate llamado");

        if (user == null)
        {
            Debug.LogError("User es null en GrenadeAbility");
            return false;
        }

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector3 direction =
            (mouseWorld - user.transform.position).normalized;

        Vector3 spawnPos =
            user.transform.position + direction * 0.6f;

        spawnPos.z = 0f;

        GameObject grenade =
            Instantiate(grenadePrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = direction * throwForce;

        return true;   // 🔥 Se consume
    }
}