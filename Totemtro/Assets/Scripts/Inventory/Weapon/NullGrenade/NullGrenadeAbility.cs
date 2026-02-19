using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Null Grenade")]
public class NullGrenadeAbility : ActiveAbilityBase
{
    public GameObject nullGrenadePrefab;
    public float throwForce = 6f;

    protected override void Activate()
    {
        GameObject grenade = Instantiate(nullGrenadePrefab, owner.transform.position, Quaternion.identity);

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - owner.transform.position).normalized;

        rb.linearVelocity = direction * throwForce;
    }
}
