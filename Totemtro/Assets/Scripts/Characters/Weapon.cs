using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    public WeaponData currentWeapon;
    public SpriteRenderer spriteRenderer;
    public LineRenderer chainRenderer;

    float lastAttackTime;

    // ===== GRIM COMBO =====
    HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
    public bool isAttacking = false;

    // ===== VEX COMBO =====
    int vexComboStep = 0;
    float lastVexAttackTime = 0f;
    float comboResetTime = 3f;

    Vector2 lastVexDirection = Vector2.right;


    public void SetWeapon(WeaponData data)
    {
        currentWeapon = data;
        spriteRenderer.sprite = data.weaponSprite;

        if (chainRenderer != null)
            chainRenderer.enabled = false;
    }

    void Update()
    {
        if (currentWeapon == null) return;

        if (Input.GetMouseButton(0) && !isAttacking)
        {
            if (Time.time >= lastAttackTime + 1f / currentWeapon.fireRate)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Projectile:
                ShootProjectile();
                break;

            case WeaponType.MeleeArc:
                DoMeleeAttack();
                break;

            case WeaponType.MurrayAnchor:
                StartCoroutine(MurrayAnchorAttack());
                break;

            case WeaponType.GrimRuneBurst:
                StartCoroutine(GrimRuneWaveAttack());
                break;

            case WeaponType.ConeProjectile:
                VexComboAttack();
                break;

        }
    }

    public int GetVexStep()
    {
        return vexComboStep;
    }


    // =====================================================
    // 🔫 PROYECTIL
    // =====================================================

    void ShootProjectile()
    {
        Vector2 shootDirection =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.position).normalized;

        PlayerMovement movement = GetComponentInParent<PlayerMovement>();
        movement.ApplyRecoil(shootDirection, 3f);

        GameObject proj = Instantiate(
            currentWeapon.projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        proj.GetComponent<Projectile>()
            .Initialize(currentWeapon.damage,
                        currentWeapon.projectileSpeed,
                        currentWeapon.range,
                        shootDirection);
    }

    // =====================================================
    // ⚔️ MELEE NORMAL
    // =====================================================

    void DoMeleeAttack()
    {
        Vector2 attackDirection =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.position).normalized;

        float radius = currentWeapon.meleeRadius;
        float angle = currentWeapon.meleeAngle;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 dirToEnemy =
                (hit.transform.position - transform.position).normalized;

            float enemyAngle = Vector2.Angle(attackDirection, dirToEnemy);

            if (enemyAngle <= angle / 2f)
            {
                hit.GetComponent<Enemy>()
                   .TakeDamage(currentWeapon.damage);
            }
        }
    }

    // =====================================================
    // ⚓ MURRAY ANCHOR
    // =====================================================

    IEnumerator MurrayAnchorAttack()
    {
        damagedEnemies.Clear();

        Debug.Log(chainRenderer);
        isAttacking = true;

        if (chainRenderer != null)
        {
            chainRenderer.enabled = true;
            Debug.Log("Chain ON");
        }




        float openTime = 0.25f;
        float swingTime = 0.5f;
        float returnTime = 0.25f;

        float radius = 0.5f;
        float coneAngle = 45f; // +10 / -10

        Vector3 origin = Vector3.zero;

        // 🔒 Bloquear dirección
        Vector2 attackDirection =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.parent.position).normalized;

        float baseAngle =
            Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        float plusAngle = baseAngle + (coneAngle / 2f);
        float minusAngle = baseAngle - (coneAngle / 2f);

        Vector2 plusPos = new Vector2(
            Mathf.Cos(plusAngle * Mathf.Deg2Rad),
            Mathf.Sin(plusAngle * Mathf.Deg2Rad)
        ) * radius;

        Vector2 minusPos = new Vector2(
            Mathf.Cos(minusAngle * Mathf.Deg2Rad),
            Mathf.Sin(minusAngle * Mathf.Deg2Rad)
        ) * radius;

        float timer = 0f;

        // 1️⃣ Inicio → +10°
        while (timer < openTime)
        {
            float t = timer / openTime;
            transform.localPosition = Vector3.Lerp(origin, plusPos, t);

            UpdateChain();
            ApplyAnchorDamage();

            timer += Time.deltaTime;
            yield return null;
        }

        // 2️⃣ +10° → -10°
        timer = 0f;
        while (timer < swingTime)
        {
            float t = timer / swingTime;
            transform.localPosition = Vector3.Lerp(plusPos, minusPos, t);

            UpdateChain();
            ApplyAnchorDamage();

            timer += Time.deltaTime;
            yield return null;
        }

        // 3️⃣ -10° → Inicio
        timer = 0f;
        while (timer < returnTime)
        {
            float t = timer / returnTime;
            transform.localPosition = Vector3.Lerp(minusPos, origin, t);

            UpdateChain();
            ApplyAnchorDamage();

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = origin;

        if (chainRenderer != null)
            chainRenderer.enabled = false;
        isAttacking = false;
    }

    // =====================================================
    // 💥 DAÑO FÍSICO REAL
    // =====================================================

    void ApplyAnchorDamage()
    {
        float anchorRadius = 0.25f;
        float chainWidth = 0.15f;

        Vector2 originWorld = transform.parent.position;
        Vector2 anchorWorld = transform.position;

        // --- Ancla ---
        Collider2D[] anchorHits =
            Physics2D.OverlapCircleAll(anchorWorld, anchorRadius);

        foreach (var hit in anchorHits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;
            if (damagedEnemies.Contains(enemy)) continue;

            enemy.TakeDamage(currentWeapon.damage);
            damagedEnemies.Add(enemy);
        }

        // --- Cadena ---
        Vector2 direction = anchorWorld - originWorld;
        float distance = direction.magnitude;

        RaycastHit2D[] chainHits =
            Physics2D.CapsuleCastAll(
                originWorld,
                new Vector2(distance, chainWidth),
                CapsuleDirection2D.Horizontal,
                Vector2.SignedAngle(Vector2.right, direction),
                Vector2.zero
            );

        foreach (var hit in chainHits)
        {
            if (!hit.collider.CompareTag("Enemy")) continue;

            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy == null) continue;
            if (damagedEnemies.Contains(enemy)) continue;

            enemy.TakeDamage(currentWeapon.damage);
            damagedEnemies.Add(enemy);
        }
    }

    // =====================================================
    // ⛓️ CADENA VISUAL
    // =====================================================

    void UpdateChain()
    {
        if (chainRenderer == null) return;

        Vector3 start = transform.parent.position;
        Vector3 end = transform.position;

        chainRenderer.SetPosition(0, start);
        chainRenderer.SetPosition(1, end);

        float distance = Vector3.Distance(start, end);

        chainRenderer.material.mainTextureScale =
            new Vector2(distance * 2f, 1f);
    }

    // =====================================================
    // 🔥 GRIM RUNE WAVE (1 → 2 → 3)
    // =====================================================

    IEnumerator GrimRuneWaveAttack()
    {
        isAttacking = true;

        PlayerMovement movement = GetComponentInParent<PlayerMovement>();
        Rigidbody2D rb = movement.GetComponent<Rigidbody2D>();

        Vector2 rawDirection =
    (Camera.main.ScreenToWorldPoint(Input.mousePosition)
    - transform.parent.position);

        if (rawDirection.sqrMagnitude < 0.08f)
        {
            rawDirection = transform.right;
        }

        Vector2 attackDirection = rawDirection.normalized;


        movement.enabled = false;
        rb.linearVelocity = Vector2.zero;

        float rowSpacing = 1f;
        float sideSpacing = 1f;
        float delayBetweenRows = 0.1f;

        float startOffset = 0.8f;
        Vector2 origin = (Vector2)transform.parent.position + attackDirection * startOffset;
        Vector2 perpendicular = new Vector2(-attackDirection.y, attackDirection.x);

        // FILA 1
        SpawnRunePro(origin + attackDirection * rowSpacing);
        yield return StartCoroutine(HitStop(0.05f));
        yield return new WaitForSeconds(delayBetweenRows);

        // FILA 2
        Vector2 row2Center = origin + attackDirection * rowSpacing * 2f;

        SpawnRunePro(row2Center + perpendicular * sideSpacing * 0.5f);
        SpawnRunePro(row2Center - perpendicular * sideSpacing * 0.5f);

        yield return StartCoroutine(HitStop(0.05f));
        yield return new WaitForSeconds(delayBetweenRows);

        // FILA 3
        Vector2 row3Center = origin + attackDirection * rowSpacing * 3f;

        SpawnRunePro(row3Center);
        SpawnRunePro(row3Center + perpendicular * sideSpacing);
        SpawnRunePro(row3Center - perpendicular * sideSpacing);

        yield return StartCoroutine(HitStop(0.07f));

        yield return new WaitForSeconds(0.15f);

        movement.enabled = true;
        isAttacking = false;
    }


    void SpawnRunePro(Vector2 position)
    {
        if (currentWeapon.projectilePrefab == null) return;

        GameObject rune = Instantiate(
            currentWeapon.projectilePrefab,
            position,
            Quaternion.identity
        );

        StartCoroutine(RuneBehaviour(rune));
    }

    IEnumerator RuneBehaviour(GameObject rune)
    {
        if (rune == null) yield break;

        float duration = 0.6f;
        float damageTickRate = 0.15f;
        float timer = 0f;

        float maxScale = currentWeapon.runeScale;

        float growTime = 0.15f;
        float growTimer = 0f;

        rune.transform.localScale = Vector3.zero;

        // 🔹 EMERGE
        while (growTimer < growTime)
        {
            if (rune == null) yield break;

            float t = growTimer / growTime;
            t = t * t * (3f - 2f * t);

            rune.transform.localScale =
                Vector3.one * Mathf.Lerp(0f, maxScale, t);

            growTimer += Time.deltaTime;
            yield return null;
        }

        if (rune == null) yield break;

        rune.transform.localScale = Vector3.one * maxScale;

        float damageTimer = 0f;

        // 🔹 VIDA ACTIVA
        while (timer < duration)
        {
            if (rune == null) yield break;

            if (damageTimer <= 0f)
            {
                ApplyRuneDamage(rune.transform.position);
                damageTimer = damageTickRate;
            }

            damageTimer -= Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        if (rune == null) yield break;

        // 🔹 DESAPARECER
        float shrinkTime = 0.15f;
        float shrinkTimer = 0f;

        while (shrinkTimer < shrinkTime)
        {
            if (rune == null) yield break;

            float t = shrinkTimer / shrinkTime;

            rune.transform.localScale =
                Vector3.one * Mathf.Lerp(maxScale, 0f, t);

            shrinkTimer += Time.deltaTime;
            yield return null;
        }

        if (rune != null)
            Destroy(rune);
    }


    void ApplyRuneDamage(Vector2 position)
    {
        float radius = 0.6f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            enemy.TakeDamage(currentWeapon.damage);
        }
    }

    IEnumerator HitStop(float duration)
    {
        float originalTimeScale = Time.timeScale;

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
    }

    // =====================================================
    // VEX ATTACK
    // =====================================================

    void VexComboAttack()
    {
        // Reset combo si pasa demasiado tiempo
        if (Time.time > lastVexAttackTime + comboResetTime)
            vexComboStep = 0;

        lastVexAttackTime = Time.time;

        Vector2 rawDirection =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - transform.parent.position);

        if (rawDirection.sqrMagnitude > 0.01f)
            lastVexDirection = rawDirection.normalized;

        Vector2 direction = lastVexDirection;

        vexComboStep++;

        if (vexComboStep > 3)
            vexComboStep = 1;

        PlayerMovement movement = GetComponentInParent<PlayerMovement>();

        switch (vexComboStep)
        {
            case 1:
                ShootSingle(direction, 1f);
                movement.ApplyRecoil(direction, 2f);
                break;

            case 2:
                ShootCone(direction, 2, 18f, 1f);
                movement.ApplyRecoil(direction, 3f);
                break;

            case 3:
                ShootCone(direction, 3, 35f, 1.4f);
                movement.ApplyRecoil(direction, 4.5f);
                StartCoroutine(VexFinalFlash());
                break;
        }
    }

    void ShootSingle(Vector2 direction, float damageMultiplier)
    {
        SpawnProjectile(direction, damageMultiplier);

        // Solo si es Tro (pistola)
        if (currentWeapon.weaponType == WeaponType.Projectile
            && currentWeapon.aimLength <= 3f) // pistola corta
        {
            StartCoroutine(PistolKickVisual());
        }
    }

    IEnumerator PistolKickVisual()
    {
        Vector3 originalPos = spriteRenderer.transform.localPosition;

        spriteRenderer.transform.localPosition += Vector3.left * 0.08f;

        yield return new WaitForSeconds(0.04f);

        spriteRenderer.transform.localPosition = originalPos;
    }



    void ShootCone(Vector2 direction, int count, float spreadAngle, float damageMultiplier)
    {
        float halfSpread = spreadAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float angleOffset = Mathf.Lerp(-halfSpread, halfSpread, t);

            Vector2 rotatedDir = RotateVector(direction, angleOffset);

            SpawnProjectile(rotatedDir, damageMultiplier);
        }
    }

    void SpawnProjectile(Vector2 direction, float damageMultiplier)
    {
        GameObject proj = Instantiate(
            currentWeapon.projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        proj.GetComponent<Projectile>()
            .Initialize(
                currentWeapon.damage * damageMultiplier,
                currentWeapon.projectileSpeed,
                currentWeapon.range,
                direction
            );
    }

    IEnumerator VexFinalFlash()
    {
        // Hitstop corto
        float originalTime = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.04f);
        Time.timeScale = originalTime;

        // Micro shake
        Camera cam = Camera.main;
        Vector3 originalPos = cam.transform.position;

        float shakeTime = 0.1f;
        float timer = 0f;

        while (timer < shakeTime)
        {
            cam.transform.position =
                originalPos + (Vector3)Random.insideUnitCircle * 0.08f;

            timer += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = originalPos;
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

}
