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

        if (GameInputLock.IsLocked)
        {
            movement = Vector2.zero;
            return;
        }

        if (InputKeyBindings.Instance != null)
        {
            movement.x = InputKeyBindings.Instance.GetHorizontalAxis();
            movement.y = InputKeyBindings.Instance.GetVerticalAxis();
        }
        else
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

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
        if (hasDash)
        {
            bool dashPressed = InputKeyBindings.Instance != null
                ? InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Dash)
                : Input.GetKeyDown(KeyCode.Space);

            if (dashPressed && Time.time >= lastDashTime + dashCooldown)
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

        pullForce = Vector2.ClampMagnitude(pullForce, maxPullForce);
    }

    void PerformDash()
    {
        lastDashTime = Time.time;

        Vector2 dashDir = movement != Vector2.zero
            ? movement.normalized
            : Vector2.up;

        rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);
    }

    // =========================
    // PUBLIC API
    // =========================

    public void AddRecoil(Vector2 force)
    {
        recoilOffset += force;
    }

    public void AddPull(Vector2 force)
    {
        pullForce += force;
    }

    /// <summary>
    /// Alias de AddPull — usado por NullSphere.
    /// </summary>
    public void ApplyPull(Vector2 force)
    {
        pullForce += force;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(multiplier, duration));
    }

    /// <summary>
    /// Fija el multiplicador de velocidad directamente (sin temporizador).
    /// Usado por StatusEffectController para slow manual con control externo.
    /// </summary>
    public void SetSlowMultiplier(float multiplier)
    {
        // Cancelar cualquier slow temporal activo
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
            slowRoutine = null;
        }

        slowMultiplier = multiplier;
    }

    IEnumerator SlowRoutine(float multiplier, float duration)
    {
        slowMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1f;
    }
}