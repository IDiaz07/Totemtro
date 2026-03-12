using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    public WeaponData currentWeapon;     // 👈 BASE DATA
    PlayerStats playerStats;             // 👈 MODIFICADORES

    public SpriteRenderer spriteRenderer;
    public LineRenderer chainRenderer;

    public bool isAiming = false;
    float lastAttackTime;
    public Transform firePoint;

    [Header("Shoot Settings")]
    public float shootMovementLockTime = 0.35f;

    [Header("Visual Recoil")]
    public float recoilRotationAmount = 6f;
    public float recoilReturnSpeed = 12f;

    Quaternion originalPlayerRotation;
    bool isRecoiling = false;

    public float CooldownRemaining { get; private set; }
    public float CurrentCooldownDuration { get; private set; }

    public float SecondaryCooldownRemaining { get; private set; }
    public float SecondaryCooldownDuration { get; private set; }

    public static System.Action OnPlayerShot;

    VexAttack vexAttack;
    MurrayAttack murrayAttack;
    KaelAttack kaelAttack;
    SeleneAttack seleneAttack;

    // =================================
    // TOTEM MODIFIERS
    // =================================
    [HideInInspector] public bool hasDualFire = false;
    [HideInInspector] public bool hasTripleShot = false;

    // =================================
    // INTERNAL
    // =================================
    HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
    public bool isAttacking = false;

    [Header("Layering")]
    public int weaponBehindOrder = -1;
    public int weaponFrontOrder = 2;

    public void SetWeapon(WeaponData data)
    {
        currentWeapon = data;
        spriteRenderer.sprite = data.weaponSprite;

        if (chainRenderer != null)
            chainRenderer.enabled = false;

        playerStats?.Initialize();
    }


    void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();

        murrayAttack = GetComponentInParent<MurrayAttack>();
        kaelAttack = GetComponentInParent<KaelAttack>();
        vexAttack = GetComponentInParent<VexAttack>();
        seleneAttack = GetComponentInParent<SeleneAttack>();
    }


    void Update()
    {
        if (currentWeapon == null || playerStats == null)
            return;

        UpdateWeaponLayer();

        float cooldown = 1f / playerStats.FireRate;

        // =====================================
        // AUTOMÁTICO (SMG)
        // =====================================
        if (currentWeapon.isAutomatic)
        {
            // Activar aim mientras mantienes click
            if (Input.GetMouseButton(0))
            {
                isAiming = true;

                if (Time.time >= lastAttackTime + cooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;

                    CurrentCooldownDuration = cooldown;
                    CooldownRemaining = cooldown;
                }
            }
            else
            {
                isAiming = false;
            }
        }
        else
        {
            // =====================================
            // SEMIAUTO (RIFLE / OTROS)
            // =====================================

            if (Input.GetMouseButtonDown(0))
            {
                isAiming = true;
            }

            if (Input.GetMouseButtonUp(0) && isAiming)
            {
                if (Time.time >= lastAttackTime + cooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;

                    CurrentCooldownDuration = cooldown;
                    CooldownRemaining = cooldown;
                }

                isAiming = false;
            }
        }

        // =====================================
        // COOLDOWN VISUAL
        // =====================================
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;

            if (CooldownRemaining < 0f)
                CooldownRemaining = 0f;
        }

        if (SecondaryCooldownRemaining > 0f)
        {
            SecondaryCooldownRemaining -= Time.deltaTime;

            if (SecondaryCooldownRemaining < 0f)
                SecondaryCooldownRemaining = 0f;
        }

        HandleSecondaryAbility();
    }

    void HandleSecondaryAbility()
    {
        if (currentWeapon == null) return;

        // 🔴 ORIN gestiona su propio click derecho
        if (currentWeapon.weaponType == WeaponType.OrinBurst)
            return;

        if (!Input.GetMouseButtonUp(1))
            return;

        if (currentWeapon.weaponType == WeaponType.KaelBlade)
        {
            if (kaelAttack != null && kaelAttack.DashAttack())
            {
                OnPlayerShot?.Invoke();

                SecondaryCooldownDuration = kaelAttack.dashCooldown;
                SecondaryCooldownRemaining = kaelAttack.dashCooldown;
            }
        }

        if (currentWeapon.weaponType == WeaponType.MurrayAnchor)
        {
            if (murrayAttack != null)
            {
                OnPlayerShot?.Invoke();

                murrayAttack.CannonShot();

                SecondaryCooldownDuration = murrayAttack.shotgunCooldown;
                SecondaryCooldownRemaining = murrayAttack.shotgunCooldown;
            }
        }

        if (currentWeapon.weaponType == WeaponType.SeleneProjectile)
        {
            float seleneCooldown = 1f / playerStats.FireRate;

            if (Time.time >= lastAttackTime + seleneCooldown)
            {
                OnPlayerShot?.Invoke();

                seleneAttack.SecondaryAttack();

                lastAttackTime = Time.time;

                CurrentCooldownDuration = seleneCooldown;
                CooldownRemaining = seleneCooldown;
            }
        }
    }

    void UpdateWeaponLayer()
    {
        if (spriteRenderer == null)
            return;

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 🔥 Comparar altura
        if (mouseWorld.y > transform.parent.position.y)
        {
            // Mouse arriba → arma detrás
            spriteRenderer.sortingOrder = weaponBehindOrder;
        }
        else
        {
            // Mouse abajo → arma delante
            spriteRenderer.sortingOrder = weaponFrontOrder;
        }
    }

    void Attack()
    {
        if (currentWeapon == null) return;

        OnPlayerShot?.Invoke();

        switch (currentWeapon.weaponType)
        {
            case WeaponType.Projectile:
            case WeaponType.OrinBurst:
                ShootProjectile();
                break;

            case WeaponType.MurrayAnchor:
                if (murrayAttack != null)
                    murrayAttack.AnchorAttack();
                break;

            case WeaponType.GrimRuneBurst:
                StartCoroutine(GrimRuneWaveAttack());
                break;

            case WeaponType.VexProyectile:
                if (vexAttack != null)
                    vexAttack.ShootVex();
                break;

            case WeaponType.NyraBloodOrb:
                ExecuteNyraAttack();
                break;

            case WeaponType.KaelBlade:
                if (kaelAttack != null)
                    kaelAttack.NormalAttack();
                else
                    Debug.LogWarning("KaelAttack component missing");
                break;

            case WeaponType.SeleneProjectile:
                if (seleneAttack != null)
                    seleneAttack.NormalAttack();
                break;

            default:
                Debug.LogWarning("WeaponType not handled: " + currentWeapon.weaponType);
                break;
        }
    }

    // =====================================================
    // 🔫 SHOOT PROJECTILE (WITH TOTEMS)
    // =====================================================

    void ShootProjectile()
    {
        if (firePoint == null || currentWeapon.projectilePrefab == null)
            return;

        Vector2 shootDirection =
            (Camera.main.ScreenToWorldPoint(Input.mousePosition)
            - firePoint.position).normalized;

        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        Vector2 playerVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

        int totalProjectiles = 1 + playerStats.ExtraProjectiles;
        float spread = 15f;

        for (int i = 0; i < totalProjectiles; i++)
        {
            float angleOffset = 0f;

            if (totalProjectiles > 1)
            {
                float totalSpread = spread * (totalProjectiles - 1);
                angleOffset = -totalSpread / 2f + spread * i;
            }

            Vector2 dir = RotateVector(shootDirection, angleOffset);

            GameObject proj = Instantiate(
                currentWeapon.projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            Projectile p = proj.GetComponent<Projectile>();

            p.Initialize(
                playerStats.Damage,
                playerStats.ProjectileSpeed,
                currentWeapon.range,
                dir,
                playerVelocity * 0.4f,
                false,
                playerStats.Pierce,
                playerStats.Ricochet
            );

            if (Random.value < playerStats.dualFireChance)
            {
                Vector2 slightOffset = RotateVector(dir, Random.Range(-6f, 6f));

                GameObject extra = Instantiate(
                    currentWeapon.projectilePrefab,
                    firePoint.position,
                    Quaternion.identity
                );

                Projectile ep = extra.GetComponent<Projectile>();

                ep.Initialize(
                    playerStats.Damage,
                    playerStats.ProjectileSpeed,
                    currentWeapon.range,
                    slightOffset,
                    playerVelocity * 0.4f,
                    false,
                    playerStats.Pierce,
                    playerStats.Ricochet
                );
            }
        }
    }



    void SpawnSingleProjectile(Vector2 direction, Vector2 inheritedVelocity)
    {
        GameObject proj = Instantiate(
            currentWeapon.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                playerStats.Damage,
                playerStats.ProjectileSpeed,
                currentWeapon.range,
                direction,
                inheritedVelocity * 0.4f,
                false
            );
        }
    }

    void SpawnSpreadProjectile(Vector2 baseDirection, float angle, Vector2 inheritedVelocity)
    {
        Vector2 left = RotateVector(baseDirection, -angle);
        Vector2 right = RotateVector(baseDirection, angle);

        SpawnSingleProjectile(left, inheritedVelocity);
        SpawnSingleProjectile(right, inheritedVelocity);
    }

    IEnumerator LockPlayerMovement(float duration)
    {
        PlayerMovement movement = GetComponentInParent<PlayerMovement>();
        if (movement == null) yield break;

        movement.enabled = false;
        yield return new WaitForSeconds(duration);
        movement.enabled = true;
    }

    IEnumerator VisualRecoil(Vector2 shootDirection)
    {
        if (isRecoiling) yield break;
        isRecoiling = true;

        Transform player = transform.parent;
        originalPlayerRotation = player.rotation;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        float recoilAngle = angle + 180f;
        float finalZ = Mathf.Sin(recoilAngle * Mathf.Deg2Rad) * recoilRotationAmount;

        player.rotation = Quaternion.Euler(0f, 0f, finalZ);

        yield return new WaitForSeconds(shootMovementLockTime);

        float t = 0f;
        while (t < 1f)
        {
            player.rotation = Quaternion.Lerp(
                player.rotation,
                originalPlayerRotation,
                t
            );

            t += Time.deltaTime * recoilReturnSpeed;
            yield return null;
        }

        player.rotation = originalPlayerRotation;
        isRecoiling = false;
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

        float openTime = currentWeapon.murrayOpenTime;
        float swingTime = currentWeapon.murraySwingTime;
        float returnTime = currentWeapon.murrayReturnTime;

        float radius = currentWeapon.murrayRadius;
        float coneAngle = currentWeapon.murrayConeAngle;

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
        float anchorRadius = currentWeapon.murrayAnchorDamageRadius;
        float chainWidth = currentWeapon.murrayChainWidth;

        Vector2 originWorld = transform.parent.position;
        Vector2 anchorWorld = transform.position;

        // --- ANCLA ---
        Collider2D[] anchorHits =
            Physics2D.OverlapCircleAll(anchorWorld, anchorRadius);

        foreach (var hit in anchorHits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;
            if (damagedEnemies.Contains(enemy)) continue;

            Vector2 hitDir =
                ((Vector2)enemy.transform.position - anchorWorld).normalized;

            enemy.TakeDamage(
                playerStats.Damage,
                hitDir,
                false
            );

            damagedEnemies.Add(enemy);
        }

        // --- CADENA ---
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

            Vector2 hitDir =
                ((Vector2)enemy.transform.position - anchorWorld).normalized;

            enemy.TakeDamage(
                playerStats.Damage,
                hitDir,
                false
            );

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

        // 🔥 DAÑO UNA SOLA VEZ
        ApplyRuneDamageOnce(rune.transform.position);

        // 🔹 VIDA VISUAL (pero ya no hace daño)
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

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

    void ApplyRuneDamageOnce(Vector2 position)
    {
        float radius = 0.6f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            Vector2 hitDir =
                ((Vector2)enemy.transform.position - position).normalized;

            enemy.TakeDamage(playerStats.Damage, hitDir, false);
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

    IEnumerator PistolKickVisual()
    {
        Vector3 originalPos = spriteRenderer.transform.localPosition;

        spriteRenderer.transform.localPosition += Vector3.left * 0.08f;

        yield return new WaitForSeconds(0.04f);

        spriteRenderer.transform.localPosition = originalPos;
    }


    void SpawnProjectile(Vector2 direction, float damageMultiplier)
    {
        if (firePoint == null) return;
        if (currentWeapon.projectilePrefab == null) return;

        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        Vector2 playerVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

        GameObject proj = Instantiate(
            currentWeapon.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                playerStats.Damage * damageMultiplier,
                playerStats.ProjectileSpeed,
                currentWeapon.range,
                direction.normalized,
                playerVelocity * 0.4f,
                false,
                playerStats.Pierce,
                playerStats.Ricochet
            );
        }
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


    // =====================================================
    // NYRA ATTACK
    // =====================================================

    void ExecuteNyraAttack()
    {
        NyraAttack nyra = GetComponent<NyraAttack>();
        if (nyra == null) return;

        nyra.Execute(currentWeapon, firePoint);
    }
}