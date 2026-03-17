using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WeaponYSort : MonoBehaviour
{
    public Transform playerBody;

    SpriteRenderer weaponSR;
    SpriteRenderer bodySR;
    Camera cam;

    void Awake()
    {
        weaponSR = GetComponent<SpriteRenderer>();
        bodySR = playerBody.GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (bodySR == null) return;

        int baseOrder = bodySR.sortingOrder;

        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        // ratón arriba o abajo del jugador
        bool aimingUp = mouse.y > playerBody.position.y;

        if (aimingUp)
            weaponSR.sortingOrder = baseOrder - 1;
        else
            weaponSR.sortingOrder = baseOrder + 1;
    }
}