using UnityEngine;
using System.Collections;

public class ExtractionHelicopter : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer bladeRenderer;
    public SpriteRenderer tailBladeRenderer;

    [Header("Rope")]
    public LineRenderer ropeRenderer;

    [Header("Main Blade Rotation")]
    public float bladeMaxSpeed = 2400f;
    public float bladeStartUpTime = 1.2f;
    public float bladeSlowDownTime = 0.8f;

    [Header("Tail Blade Rotation")]
    public float tailBladeMaxSpeed = 3200f;
    public float tailBladeStartUpTime = 0.8f;
    public float tailBladeSlowDownTime = 0.6f;

    [Header("Blade Visual")]
    [Range(0f, 1f)] public float bladeMinAlpha = 0.15f;
    [Range(0f, 1f)] public float bladeMaxAlpha = 0.6f;
    public float bladeStretchMin = 1f;
    public float bladeStretchMax = 1.08f;

    [Header("Tail Blade Visual")]
    [Range(0f, 1f)] public float tailBladeMinAlpha = 0.1f;
    [Range(0f, 1f)] public float tailBladeMaxAlpha = 0.5f;
    public float tailBladeStretchMin = 1f;
    public float tailBladeStretchMax = 1.05f;

    [Header("Entrance")]
    public float enterFromHeight = 20f;
    public float descendDuration = 1.5f;
    public float hoverHeight = 8f;

    [Header("Rope Drop")]
    public float ropeDropDuration = 0.6f;
    public float ropeLength = 6f;

    [Header("Player Climb")]
    public float climbDuration = 1.8f;

    [Header("Exit")]
    public float exitHeight = 25f;
    public float exitDuration = 2f;

    [Header("Shake")]
    public float landShakeIntensity = 0.12f;
    public float landShakeDuration = 0.3f;

    [Header("Sway")]
    public float swayAmount = 0.15f;
    public float swaySpeed = 2f;
    public float verticalBobAmount = 0.06f;
    public float verticalBobSpeed = 3f;

    [Header("Body Tilt")]
    public float hoverTiltAmount = 1.5f;
    public float hoverTiltSpeed = 1.2f;

    // =========================================
    // INTERNAL
    // =========================================

    float bladeAngle;
    float currentBladeSpeed;
    float targetBladeSpeed;

    float tailBladeAngle;
    float currentTailBladeSpeed;
    float targetTailBladeSpeed;

    Vector3 hoverPos;
    bool isHovering = false;
    Vector3 bladeOriginalScale;
    Vector3 tailBladeOriginalScale;

    void Start()
    {
        if (bladeRenderer != null)
            bladeOriginalScale = bladeRenderer.transform.localScale;

        if (tailBladeRenderer != null)
            tailBladeOriginalScale = tailBladeRenderer.transform.localScale;
    }

    void Update()
    {
        UpdateBladeRotation();
        UpdateBladeVisuals();
        UpdateTailBladeRotation();
        UpdateTailBladeVisuals();
        UpdateHover();
    }

    // =========================================
    // MAIN BLADE — ROTACIÓN
    // =========================================

    void UpdateBladeRotation()
    {
        if (bladeRenderer == null) return;

        float accelTime = targetBladeSpeed > currentBladeSpeed
            ? bladeStartUpTime
            : bladeSlowDownTime;

        currentBladeSpeed = Mathf.MoveTowards(
            currentBladeSpeed,
            targetBladeSpeed,
            (bladeMaxSpeed / accelTime) * Time.deltaTime
        );

        bladeAngle += currentBladeSpeed * Time.deltaTime;
        bladeRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, bladeAngle);
    }

    void UpdateBladeVisuals()
    {
        if (bladeRenderer == null) return;

        float speedRatio = Mathf.Clamp01(currentBladeSpeed / bladeMaxSpeed);

        float alpha = Mathf.Lerp(bladeMaxAlpha, bladeMinAlpha, speedRatio);
        Color col = bladeRenderer.color;
        col.a = alpha;
        bladeRenderer.color = col;

        float stretch = Mathf.Lerp(bladeStretchMin, bladeStretchMax, speedRatio);
        bladeRenderer.transform.localScale = new Vector3(
            bladeOriginalScale.x * stretch,
            bladeOriginalScale.y * stretch,
            bladeOriginalScale.z
        );
    }

    // =========================================
    // TAIL BLADE — ROTACIÓN
    // =========================================

    void UpdateTailBladeRotation()
    {
        if (tailBladeRenderer == null) return;

        float accelTime = targetTailBladeSpeed > currentTailBladeSpeed
            ? tailBladeStartUpTime
            : tailBladeSlowDownTime;

        currentTailBladeSpeed = Mathf.MoveTowards(
            currentTailBladeSpeed,
            targetTailBladeSpeed,
            (tailBladeMaxSpeed / accelTime) * Time.deltaTime
        );

        tailBladeAngle += currentTailBladeSpeed * Time.deltaTime;
        tailBladeRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, tailBladeAngle);
    }

    void UpdateTailBladeVisuals()
    {
        if (tailBladeRenderer == null) return;

        float speedRatio = Mathf.Clamp01(currentTailBladeSpeed / tailBladeMaxSpeed);

        float alpha = Mathf.Lerp(tailBladeMaxAlpha, tailBladeMinAlpha, speedRatio);
        Color col = tailBladeRenderer.color;
        col.a = alpha;
        tailBladeRenderer.color = col;

        float stretch = Mathf.Lerp(tailBladeStretchMin, tailBladeStretchMax, speedRatio);
        tailBladeRenderer.transform.localScale = new Vector3(
            tailBladeOriginalScale.x * stretch,
            tailBladeOriginalScale.y * stretch,
            tailBladeOriginalScale.z
        );
    }

    // =========================================
    // HOVER — MOVIMIENTO ORGÁNICO
    // =========================================

    void UpdateHover()
    {
        if (!isHovering) return;

        float time = Time.time;

        float swayX = Mathf.Sin(time * swaySpeed) * swayAmount;
        float bobY = Mathf.Sin(time * verticalBobSpeed) * verticalBobAmount;

        transform.position = hoverPos + new Vector3(swayX, bobY, 0f);

        float tilt = Mathf.Sin(time * hoverTiltSpeed) * hoverTiltAmount;
        bodyRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    // =========================================
    // BLADE SPEED CONTROL
    // =========================================

    void SetBladeTarget(float speed)
    {
        targetBladeSpeed = speed;
    }

    void SetTailBladeTarget(float speed)
    {
        targetTailBladeSpeed = speed;
    }

    void SetAllBladesTarget(float mainSpeed, float tailSpeed)
    {
        targetBladeSpeed = mainSpeed;
        targetTailBladeSpeed = tailSpeed;
    }

    IEnumerator WaitForBladeSpeed(float threshold)
    {
        while (currentBladeSpeed < threshold)
            yield return null;
    }

    IEnumerator WaitForBothBladesSpeed(float mainThreshold, float tailThreshold)
    {
        while (currentBladeSpeed < mainThreshold || currentTailBladeSpeed < tailThreshold)
            yield return null;
    }

    // =========================================
    // CINEMATIC SEQUENCE
    // =========================================

    public IEnumerator PlayFullSequence(Transform player, HeroController heroController, Weapon weapon)
    {
        // =========================================
        // 0. ARRANCAR HÉLICES ANTES DE ENTRAR
        // =========================================

        Vector3 targetPos = player.position + Vector3.up * hoverHeight;
        Vector3 startPos = targetPos + Vector3.up * enterFromHeight;

        transform.position = startPos;
        gameObject.SetActive(true);

        if (ropeRenderer != null)
            ropeRenderer.enabled = false;

        // Encender ambas hélices (se oye llegar)
        SetAllBladesTarget(bladeMaxSpeed, tailBladeMaxSpeed);
        yield return StartCoroutine(
            WaitForBothBladesSpeed(bladeMaxSpeed * 0.6f, tailBladeMaxSpeed * 0.5f));

        // =========================================
        // 1. HELICÓPTERO DESCIENDE
        // =========================================

        float t = 0f;
        while (t < descendDuration)
        {
            t += Time.deltaTime;
            float eased = EaseOutQuad(t / descendDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, eased);
            yield return null;
        }

        transform.position = targetPos;
        hoverPos = targetPos;
        isHovering = true;

        CameraShake.ShakeCamera(landShakeIntensity, landShakeDuration);

        yield return new WaitForSeconds(0.3f);

        // =========================================
        // 2. LANZAR CUERDA
        // =========================================

        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = true;
            ropeRenderer.positionCount = 2;

            Vector3 ropeStart = transform.position;
            Vector3 ropeEnd = transform.position + Vector3.down * ropeLength;

            t = 0f;
            while (t < ropeDropDuration)
            {
                t += Time.deltaTime;
                float progress = t / ropeDropDuration;

                Vector3 currentEnd = Vector3.Lerp(ropeStart, ropeEnd, progress);

                ropeRenderer.SetPosition(0, transform.position);
                ropeRenderer.SetPosition(1, currentEnd);

                yield return null;
            }

            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, ropeEnd);
        }

        yield return new WaitForSeconds(0.2f);

        // =========================================
        // 3. PREPARAR JUGADOR
        // =========================================

        if (weapon != null)
        {
            weapon.enabled = false;
            weapon.gameObject.SetActive(false);
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        if (heroController != null &&
            heroController.bodyRenderer != null &&
            heroController.currentHero?.directionalSprites != null)
        {
            heroController.bodyRenderer.sprite =
                heroController.currentHero.directionalSprites.BackView;
        }

        // =========================================
        // 4. JUGADOR SUBE POR LA CUERDA
        // =========================================

        Vector3 playerStart = player.position;
        Vector3 playerEnd = transform.position;

        t = 0f;
        while (t < climbDuration)
        {
            t += Time.deltaTime;
            float progress = t / climbDuration;
            float eased = EaseInQuad(progress);

            player.position = Vector3.Lerp(playerStart, playerEnd, eased);

            if (ropeRenderer != null)
            {
                float ropeBottom = Mathf.Lerp(
                    playerStart.y - 0.5f,
                    transform.position.y,
                    eased
                );

                ropeRenderer.SetPosition(0, transform.position);
                ropeRenderer.SetPosition(1,
                    new Vector3(transform.position.x, ropeBottom, 0f));
            }

            if (progress > 0.7f)
            {
                float shrink = Mathf.InverseLerp(0.7f, 1f, progress);
                float scale = Mathf.Lerp(1f, 0f, shrink);
                player.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        player.gameObject.SetActive(false);

        if (ropeRenderer != null)
            ropeRenderer.enabled = false;

        yield return new WaitForSeconds(0.4f);

        // =========================================
        // 5. HELICÓPTERO SE VA
        // =========================================

        isHovering = false;

        StartCoroutine(TiltBodyOnExit());

        Vector3 exitPos = transform.position + Vector3.up * exitHeight;
        Vector3 fromPos = transform.position;

        t = 0f;
        while (t < exitDuration)
        {
            t += Time.deltaTime;
            float eased = EaseInQuad(t / exitDuration);
            transform.position = Vector3.Lerp(fromPos, exitPos, eased);
            yield return null;
        }

        // Apagar ambas hélices al final
        SetAllBladesTarget(0f, 0f);
    }

    IEnumerator TiltBodyOnExit()
    {
        if (bodyRenderer == null) yield break;

        float t = 0f;
        float tiltDuration = 0.5f;
        float maxTilt = -8f;

        while (t < tiltDuration)
        {
            t += Time.deltaTime;
            float tilt = Mathf.Lerp(0f, maxTilt, EaseOutQuad(t / tiltDuration));
            bodyRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            yield return null;
        }
    }

    // =========================================
    // EASING
    // =========================================

    float EaseOutQuad(float x) => 1f - (1f - x) * (1f - x);
    float EaseInQuad(float x) => x * x;
}