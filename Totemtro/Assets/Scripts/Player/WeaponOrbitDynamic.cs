using UnityEngine;

public class WeaponOrbitDynamic : MonoBehaviour
{
    public Transform weapon;
    public SpriteRenderer weaponSprite; // ← AÑADIDO

    public float minRadius = 0.12f;
    public float maxRadius = 0.38f;
    public float maxMouseDistance = 2.5f;
    public float smoothSpeed = 12f;

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 toMouse = mouseWorld - transform.position;
        float mouseDistance = toMouse.magnitude;
        Vector2 direction = toMouse.normalized;

        // 1️⃣ Normalizamos la distancia del mouse
        float t = Mathf.Clamp01(mouseDistance / maxMouseDistance);

        // 2️⃣ Calculamos el radio objetivo
        float targetRadius = Mathf.Lerp(minRadius, maxRadius, t);

        // 3️⃣ Suavizado
        float currentRadius = Mathf.Lerp(
            weapon.localPosition.magnitude,
            targetRadius,
            Time.deltaTime * smoothSpeed
        );

        // 4️⃣ Posicionamos el arma
        weapon.localPosition = direction * currentRadius;

        // 5️⃣ Rotamos el arma
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Si apuntamos a la izquierda, compensamos
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

        weapon.rotation = Quaternion.Euler(0, 0, angle);

        // 6️⃣ FLIP X (LO ÚNICO NUEVO)
        weaponSprite.flipX = direction.x < 0f;
    }
}
