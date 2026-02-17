using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    Vector2 recoilOffset;
    float recoilDecay = 12f;

    public float speed = 5f;

    // =========================
    // SLOW SYSTEM
    // =========================
    float slowMultiplier = 1f;
    Coroutine slowRoutine;

    Rigidbody2D rb;
    Vector2 movement;

    // =========================
    // DASH
    // =========================
    public bool hasDash = false;
    public float dashForce = 12f;
    public float dashCooldown = 2f;

    float lastDashTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Movimiento normal
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        // DASH
        if (hasDash && Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time >= lastDashTime + dashCooldown)
            {
                PerformDash();
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 move = movement * speed * slowMultiplier;
        move += recoilOffset;

        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

        // Recoil decay
        recoilOffset = Vector2.Lerp(
            recoilOffset,
            Vector2.zero,
            recoilDecay * Time.fixedDeltaTime
        );
    }

    void PerformDash()
    {
        Vector2 dashDir = movement;

        // Si no se está moviendo, dash hacia el mouse
        if (dashDir == Vector2.zero)
        {
            dashDir =
                (Camera.main.ScreenToWorldPoint(Input.mousePosition)
                - transform.position).normalized;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dashDir.normalized * dashForce, ForceMode2D.Impulse);

        lastDashTime = Time.time;
    }

    public void ApplyRecoil(Vector2 direction, float force)
    {
        recoilOffset += -direction.normalized * force;
    }

    public void ApplySlow(float percent, float duration)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(percent, duration));
    }

    IEnumerator SlowRoutine(float percent, float duration)
    {
        slowMultiplier = 1f - percent;

        yield return new WaitForSeconds(duration);

        slowMultiplier = 1f;
    }
}
