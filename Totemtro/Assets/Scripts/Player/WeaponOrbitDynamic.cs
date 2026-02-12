using UnityEngine;

public class WeaponOrbitDynamic : MonoBehaviour
{
    public Transform weapon;              // Transform del arma
    public SpriteRenderer weaponSprite;   // SpriteRenderer del arma

    public float minRadius = 0.12f;
    public float maxRadius = 0.38f;
    public float maxMouseDistance = 2.5f;
    public float smoothSpeed = 12f;

    Weapon weaponScript;   // Referencia al script Weapon

    void Awake()
    {
        // Busca el Weapon en el objeto hijo
        weaponScript = GetComponentInChildren<Weapon>();
    }

    void Update()
    {
        // 🔒 Si está atacando, NO actualizar rotación ni posición
        if (weaponScript != null && weaponScript.isAttacking)
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 toMouse = mouseWorld - transform.position;
        float mouseDistance = toMouse.magnitude;

        if (mouseDistance == 0f) return;

        Vector2 direction = toMouse.normalized;

        // Normalizamos distancia
        float t = Mathf.Clamp01(mouseDistance / maxMouseDistance);

        // Radio dinámico
        float targetRadius = Mathf.Lerp(minRadius, maxRadius, t);

        float currentRadius = Mathf.Lerp(
            weapon.localPosition.magnitude,
            targetRadius,
            Time.deltaTime * smoothSpeed
        );

        // Posición
        weapon.localPosition = direction * currentRadius;

        // Rotación
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (direction.x < 0f)
        {
            weaponSprite.flipX = true;
            angle += 180f;
        }
        else
        {
            weaponSprite.flipX = false;
        }

        weapon.rotation = Quaternion.Euler(0, 0, angle);
    }
}
