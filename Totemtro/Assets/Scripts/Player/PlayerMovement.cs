using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Vector2 recoilOffset;
    float recoilDecay = 12f;
    public float speed = 5f;

    Rigidbody2D rb;
    Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();
    }

    void FixedUpdate()
    {
        // Movimiento base
        Vector2 move = movement * speed;

        // Sumamos el recoil
        move += recoilOffset;

        // Aplicamos movimiento
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

        // Reducimos el recoil suavemente
        recoilOffset = Vector2.Lerp(
            recoilOffset,
            Vector2.zero,
            recoilDecay * Time.fixedDeltaTime
        );
    }


    public void ApplyRecoil(Vector2 direction, float force)
    {
        recoilOffset += -direction.normalized * force;
    }

}
