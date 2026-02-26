using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Null Grenade")]
public class NullGrenadeAbility : ActiveAbilityBase
{
    public GameObject nullGrenadePrefab;
    public float throwForce = 6f;

    protected override bool Activate(GameObject user)
    {
        if (user == null)
            return false;

        Vector3 spawnPos = user.transform.position;

        GameObject grenade =
            Instantiate(nullGrenadePrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        Vector2 direction =
            (mouseWorld - spawnPos).normalized;

        if (rb != null)
            rb.linearVelocity = direction * throwForce;

        return true;   // 🔥 Se consume
    }
}