using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Grenade")]
public class GrenadeAbility : ActiveAbilityBase
{
    public GameObject grenadePrefab;
    public float throwForce = 6f;

    protected override void Activate()
    {
        if (owner == null) return;

        GameObject grenade = Instantiate(grenadePrefab, owner.transform.position, Quaternion.identity);

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - owner.transform.position).normalized;

        rb.linearVelocity = direction * throwForce;
    }
}
