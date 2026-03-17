using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // =========================
    // MOVEMENT
    // =========================
    public float speed = 5f;

    Rigidbody2D rb;
    Vector2 movement;

    HeroController heroController;

    // =========================
    // RECOIL SYSTEM
    // =========================
    Vector2 recoilOffset;
    float recoilDecay = 12f;

    // =========================
    // PULL SYSTEM
    // =========================
    Vector2 pullForce;
    float pullDecay = 8f;
    float maxPullForce = 6f;

    // =========================
    // SLOW SYSTEM
    // =========================
    float slowMultiplier = 1f;
    Coroutine slowRoutine;

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
        heroController = GetComponent<HeroController>();
    }

    void Update()
    {
        if (GameIntroState.IsIntroPlaying)
            return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        // 🔥 ACTUALIZA DIRECCIÓN VISUAL
        if (heroController != null)
        {
            Vector2 mouseWorld =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 lookDir =
                (mouseWorld - (Vector2)transform.position).normalized;

            bool isMoving = movement != Vector2.zero;

            heroController.UpdateLookDirection(lookDir, isMoving);
        }

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
        move += pullForce;

        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

        // 🔹 Recoil decay
        recoilOffset = Vector2.Lerp(
            recoilOffset,
            Vector2.zero,
            recoilDecay * Time.fixedDeltaTime
        );

        // 🔹 Pull decay
        pullForce = Vector2.Lerp(
            pullForce,
            Vector2.zero,
            pullDecay * Time.fixedDeltaTime
        );
    }

    void PerformDash()
    {
        Vector2 dashDir = movement;

        if (dashDir == Vector2.zero)
        {
            dashDir =
                (Camera.main.ScreenToWorldPoint(Input.mousePosition)
                - transform.position).normalized;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dashDir.normalized * dashForce, ForceMode2D.Impulse);

        // 🔥 ACTUALIZA DIRECCIÓN VISUAL AL DASH
        if (heroController != null)
            heroController.UpdateDirection(dashDir);

        lastDashTime = Time.time;
    }

    // =========================
    // EXTERNAL EFFECTS
    // =========================

    public void ApplyRecoil(Vector2 direction, float force)
    {
        recoilOffset += -direction.normalized * force;
    }

    public void ApplyPull(Vector2 force)
    {
        pullForce = Vector2.ClampMagnitude(
            pullForce + force,
            maxPullForce
        );
    }

    public void ClearPull()
    {
        pullForce = Vector2.zero;
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

    public bool IsMoving()
    {
        return movement != Vector2.zero;
    }

    public void SetSlowMultiplier(float value)
    {
        slowMultiplier = value;
    }
}