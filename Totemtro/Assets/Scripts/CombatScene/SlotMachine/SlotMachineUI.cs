using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineUI : MonoBehaviour
{
    public Image[] reels;                     // Root images (cada uno actuará como "slot column")
    public List<SlotIconData> icons;

    [Header("Reward UI")]
    public GameObject rewardPopup;
    public Image rewardIcon;
    public Image rewardBackground;

    [Header("Animation")]
    public RectTransform rewardIconRect;
    public RectTransform tooltipAnchor;
    public float moveDuration = 0.4f;

    [Header("Glow")]
    public Image glow;

    [Header("Particles")]
    public ParticleSystem rewardParticles;
    public ParticleSystem legendaryParticles; // opcional: partículas especiales para legendary

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip buttonSFX;      // 🔥 BOTÓN / PALANCA
    public AudioClip spinLoopSFX;    // 🔥 LOOP RODILLOS

    public AudioClip commonSFX;
    public AudioClip rareSFX;
    public AudioClip epicSFX;
    public AudioClip legendarySFX;

    [Header("Backgrounds by Rarity")]
    public Sprite commonBG;
    public Sprite rareBG;
    public Sprite epicBG;
    public Sprite legendaryBG;

    [Header("UX extras")]
    public UIShake uiShake;
    public Camera mainCamera;
    public float zoomAmount = 0.9f;
    public float zoomDuration = 0.4f;

    [Header("Reel visuals")]
    public float spinDuration = 1.5f;          // tiempo total del spin (antes de que SlotMachine decida resultado)
    public float reelScrollSpeed = 600f;       // pixels por segundo (ajusta para velocidad visual)
    public bool useUnscaledTime = true;

    // parámetros de parada secuencial
    public float perReelStopDelay = 0.12f;
    public float reelSettleDuration = 0.22f;

    // fallback si no hay sprites definidos
    public Sprite fallbackSprite;

    [Header("Reward timing / animation")]
    public float rewardDelay = 5.5f;             // segundos hasta que aparece el reward tras resultado
    public float legendaryPopScale = 1.6f;     // escala inicial de pop para legendary
    public float legendaryPopDuration = 0.6f;  // duración pop legendary

    [Header("Loss FX")]
    public Image explosionImage;
    public float explosionDuration = 0.4f;

    private Dictionary<SlotIconType, Sprite> iconDict;
    public static SlotMachineUI Instance;
    public CustomTooltipUI customTooltip;

    ItemData currentItem;
    int currentAmount;
    bool rewardReady = false;

    SlotMachine currentMachine;

    // Estructura runtime por reel: 3 celdas (Image) y su RectTransform
    class ReelVisual
    {
        public Image rootImage;
        public RectTransform rootRect;
        public Image[] cells = new Image[3]; // 0=top,1=center,2=bottom
        public RectTransform[] cellRects = new RectTransform[3];
        public float cellHeight;
        public Coroutine spinCoroutine;
        public bool stopping = false;
        public Sprite finalSprite = null;
    }

    ReelVisual[] reelVisuals;

    // coroutines controladas
    Coroutine rewardCoroutine;
    Coroutine stopReelsCoroutine;
    Coroutine rewardDelayCoroutine;

    void Awake()
    {
        Instance = this;

        iconDict = new Dictionary<SlotIconType, Sprite>();

        if (icons != null)
        {
            foreach (var icon in icons)
            {
                if (icon != null && !iconDict.ContainsKey(icon.type))
                    iconDict.Add(icon.type, icon.sprite);
            }
        }

        InitializeReels();
        if (rewardPopup != null)
            rewardPopup.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!UILayerManager.IsOpen(UILayerManager.Layer.SlotMachine)) return;

        if (InputKeyBindings.Instance != null)
        {
            if (InputKeyBindings.Instance.GetKeyDown(InputKeyBindings.Action.Pause))
                OnCloseButton();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCloseButton();
        }
    }

    // Inicializa o crea 3 celdas por reel y asigna sprites aleatorios
    void InitializeReels()
    {
        if (reels == null || reels.Length == 0)
        {
            Debug.LogWarning("[SlotMachineUI] No hay `reels` asignados.");
            return;
        }

        reelVisuals = new ReelVisual[reels.Length];

        for (int i = 0; i < reels.Length; i++)
        {
            var rv = new ReelVisual();
            rv.rootImage = reels[i];
            if (rv.rootImage == null)
            {
                Debug.LogWarning($"[SlotMachineUI] reels[{i}] es null en el inspector.");
                continue;
            }

            rv.rootRect = rv.rootImage.GetComponent<RectTransform>();

            // --- HACER TRANSPARENTE LA RAÍZ Y EVITAR QUE OCUPE FONDO BLANCO ---
            // Quitar cualquier sprite de fondo en la imagen raíz para que no tape a las celdas
            rv.rootImage.sprite = null;
            rv.rootImage.color = new Color(1f, 1f, 1f, 0f);
            rv.rootImage.raycastTarget = false;

            // Si hay un CanvasGroup en el root y su alpha es 0, los hijos no se verán.
            var cg = rv.rootRect.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                if (cg.alpha == 0f)
                {
                    Debug.LogWarning("[SlotMachineUI] CanvasGroup.alpha == 0 en root del reel. Lo pongo a 1 para que los hijos sean visibles.");
                    cg.alpha = 1f;
                }
            }

            // Añadir RectMask2D para recortar todo lo que quede fuera del rect del reel.
            // Usamos RectMask2D porque funciona aunque la Image root sea transparente.
            if (rv.rootRect.GetComponent<UnityEngine.UI.RectMask2D>() == null)
            {
                rv.rootRect.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            }

            // calcular altura de celda basándonos en el rect del root
            rv.cellHeight = Mathf.Abs(rv.rootRect.rect.height);

            // buscar hijos Image existentes (si hay 3 o más, usarlos)
            List<Image> found = new List<Image>();
            for (int c = 0; c < rv.rootRect.childCount; c++)
            {
                var child = rv.rootRect.GetChild(c).GetComponent<Image>();
                if (child != null)
                    found.Add(child);
            }

            if (found.Count >= 3)
            {
                // usar los primeros 3 (top, center, bottom)
                for (int j = 0; j < 3; j++)
                {
                    rv.cells[j] = found[j];
                    rv.cellRects[j] = rv.cells[j].GetComponent<RectTransform>();
                }
            }
            else
            {
                // crear 3 celdas nuevas y colocarlas (top, center, bottom)
                for (int j = 0; j < 3; j++)
                {
                    GameObject go = new GameObject($"Cell{j}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    go.transform.SetParent(rv.rootRect, false);
                    var img = go.GetComponent<Image>();
                    img.preserveAspect = true;
                    img.raycastTarget = false;
                    img.maskable = true; // permitir que el RectMask2D recorte esta Image

                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 0.5f);
                    rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    // usar tamaño en Y igual a la altura del root para que el cell cubra la ranura
                    rt.sizeDelta = new Vector2(0f, rv.cellHeight);
                    rv.cells[j] = img;
                    rv.cellRects[j] = rt;
                }
            }

            // posicionar top/mid/bottom
            rv.cellRects[0].anchoredPosition = new Vector2(0f, rv.cellHeight);
            rv.cellRects[1].anchoredPosition = Vector2.zero;
            rv.cellRects[2].anchoredPosition = new Vector2(0f, -rv.cellHeight);

            // asignar sprites random iniciales y forzar alpha 1
            for (int j = 0; j < 3; j++)
            {
                var s = GetRandomSprite() ?? fallbackSprite;
                rv.cells[j].sprite = s;
                rv.cells[j].enabled = true;
                var col = rv.cells[j].color;
                if (col.a <= 0f) col.a = 1f;
                rv.cells[j].color = col;
            }

            reelVisuals[i] = rv;
        }
    }

    Sprite GetRandomSprite()
    {
        if (icons == null || icons.Count == 0) return fallbackSprite;
        var si = icons[Random.Range(0, icons.Count)];
        if (si == null) return fallbackSprite;
        if (si.sprite == null) return fallbackSprite;
        return si.sprite;
    }

    // =========================
    // SPIN START: lanza el scrolling visual por reel y espera spinDuration
    public void StartSpin(System.Action onComplete)
    {
        PlayButtonSound();

        // comenzar spin visuals por reel
        if (reelVisuals == null) InitializeReels();

        for (int i = 0; i < reelVisuals.Length; i++)
        {
            if (reelVisuals[i] == null) continue;
            if (reelVisuals[i].spinCoroutine != null)
            {
                StopCoroutine(reelVisuals[i].spinCoroutine);
                reelVisuals[i].spinCoroutine = null;
            }
            reelVisuals[i].stopping = false;
            reelVisuals[i].finalSprite = null;
            reelVisuals[i].spinCoroutine = StartCoroutine(SpinReelCoroutine(i));
        }

        StartCoroutine(SpinRoutine(onComplete));
    }

    IEnumerator SpinRoutine(System.Action onComplete)
    {
        PlaySpinLoop(true);

        float timer = 0f;
        while (timer < spinDuration)
        {
            timer += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            yield return null;
        }

        PlaySpinLoop(false);

        onComplete?.Invoke();
    }

    // Coroutine que hace scroll de las 3 celdas y recicla sprites
    IEnumerator SpinReelCoroutine(int reelIndex)
    {
        var rv = reelVisuals[reelIndex];
        if (rv == null) yield break;

        float speed = reelScrollSpeed;

        while (true)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // mover cada cell hacia abajo
            for (int c = 0; c < 3; c++)
            {
                var rect = rv.cellRects[c];
                rect.anchoredPosition += Vector2.down * speed * dt;
            }

            // si alguna cell ha pasado por debajo del bottom limit, reciclarla arriba
            for (int c = 0; c < 3; c++)
            {
                var rect = rv.cellRects[c];
                if (rect.anchoredPosition.y < -rv.cellHeight - 5f)
                {
                    // mover al top
                    rect.anchoredPosition += Vector2.up * rv.cellHeight * 3f;

                    // asignar nuevo sprite (aleatorio) solo si no estamos en stopping con finalSprite solicitada
                    if (!rv.stopping)
                        rv.cells[c].sprite = GetRandomSprite() ?? fallbackSprite;
                    else
                        rv.cells[c].sprite = GetRandomSprite() ?? fallbackSprite;
                }
            }

            // si nos han pedido parar este reel, comprobamos si center cell está cerca de 0 para fijar finalSprite
            if (rv.stopping && rv.finalSprite != null)
            {
                float centerY = rv.cellRects[1].anchoredPosition.y;
                if (Mathf.Abs(centerY) < 8f)
                {
                    // snap posiciones exactas top=cellHeight, center=0, bottom=-cellHeight
                    rv.cellRects[0].anchoredPosition = new Vector2(0f, rv.cellHeight);
                    rv.cellRects[1].anchoredPosition = Vector2.zero;
                    rv.cellRects[2].anchoredPosition = new Vector2(0f, -rv.cellHeight);

                    // poner final sprite en center
                    rv.cells[1].sprite = rv.finalSprite;

                    // parar coroutine
                    rv.spinCoroutine = null;
                    yield break;
                }
            }

            yield return null;
        }
    }

    // =========================
    // Llamado por SlotMachine → queremos parar secuencialmente cada reel y fijar el resultado en el centro
    public void SetFinalResult(SlotIconType result)
    {
        if (stopReelsCoroutine != null)
        {
            StopCoroutine(stopReelsCoroutine);
            stopReelsCoroutine = null;
        }

        stopReelsCoroutine = StartCoroutine(StopReelsRoutine(result));
    }

    IEnumerator StopReelsRoutine(SlotIconType result)
    {
        // elegir sprite final para el resultado
        Sprite finalSprite = null;
        if (iconDict != null && iconDict.ContainsKey(result))
            finalSprite = iconDict[result];
        if (finalSprite == null)
            finalSprite = fallbackSprite;

        for (int i = 0; i < reelVisuals.Length; i++)
        {
            var rv = reelVisuals[i];
            if (rv == null) continue;

            // solicitar parada para este reel
            rv.stopping = true;
            rv.finalSprite = finalSprite;

            // esperar un tiempo para dar sensación secuencial y permitir que el reel "asiente"
            float waited = 0f;
            while (waited < reelSettleDuration)
            {
                waited += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                yield return null;
            }

            // esperar hasta que el reel haya terminado (spin coroutine saldrá cuando centre)
            float stopTimeout = 1.5f;
            float elapsed = 0f;
            while (rv.spinCoroutine != null && elapsed < stopTimeout)
            {
                elapsed += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                yield return null;
            }

            // si por timeout sigue corriendo, forzamos alignment:
            if (rv.spinCoroutine != null)
            {
                // forzar posiciones y sprite center
                rv.cellRects[0].anchoredPosition = new Vector2(0f, rv.cellHeight);
                rv.cellRects[1].anchoredPosition = Vector2.zero;
                rv.cellRects[2].anchoredPosition = new Vector2(0f, -rv.cellHeight);
                rv.cells[1].sprite = finalSprite;

                // detener coroutine si sigue
                if (rv.spinCoroutine != null)
                {
                    StopCoroutine(rv.spinCoroutine);
                    rv.spinCoroutine = null;
                }
            }

            // delay antes de parar el siguiente reel
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(perReelStopDelay);
            else
                yield return new WaitForSeconds(perReelStopDelay);
        }

        stopReelsCoroutine = null;
    }

    // NUEVA API: aceptar resultados por rodillo y arrancar la rutina de parada secuencial usando esos resultados
    public void SetFinalResults(SlotIconType[] results)
    {
        if (results == null || results.Length == 0) return;

        if (stopReelsCoroutine != null)
        {
            StopCoroutine(stopReelsCoroutine);
            stopReelsCoroutine = null;
        }

        stopReelsCoroutine = StartCoroutine(StopReelsRoutinePerReel(results));
    }

    IEnumerator StopReelsRoutinePerReel(SlotIconType[] results)
    {
        int count = Mathf.Min(results.Length, reelVisuals != null ? reelVisuals.Length : 0);

        for (int i = 0; i < count; i++)
        {
            var rv = reelVisuals[i];
            if (rv == null) continue;

            // asignar sprite final para este rodillo (si existe en iconDict)
            Sprite finalSprite = null;
            if (iconDict != null && iconDict.ContainsKey(results[i]))
                finalSprite = iconDict[results[i]];

            if (finalSprite == null)
                finalSprite = fallbackSprite;

            rv.stopping = true;
            rv.finalSprite = finalSprite;

            // esperar un tiempo para sensación secuencial
            float waited = 0f;
            while (waited < reelSettleDuration)
            {
                waited += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                yield return null;
            }

            // esperar a que el reel centre o forzar snap tras timeout
            float stopTimeout = 1.5f;
            float elapsed = 0f;
            while (rv.spinCoroutine != null && elapsed < stopTimeout)
            {
                elapsed += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                yield return null;
            }

            if (rv.spinCoroutine != null)
            {
                // forzar posiciones y sprite center
                rv.cellRects[0].anchoredPosition = new Vector2(0f, rv.cellHeight);
                rv.cellRects[1].anchoredPosition = Vector2.zero;
                rv.cellRects[2].anchoredPosition = new Vector2(0f, -rv.cellHeight);
                rv.cells[1].sprite = finalSprite;

                if (rv.spinCoroutine != null)
                {
                    StopCoroutine(rv.spinCoroutine);
                    rv.spinCoroutine = null;
                }
            }

            // pequeño delay antes del siguiente rodillo
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(perReelStopDelay);
            else
                yield return new WaitForSeconds(perReelStopDelay);
        }

        stopReelsCoroutine = null;
    }

    // =========================
    // Mostrar reward: ahora con delay y animación especial para legendary
    public void ShowReward(ItemData item, InventorySlot slot, SlotIconType type)
    {
        if (customTooltip != null)
            customTooltip.Hide();

        if (rewardPopup == null || item == null) return;

        // guardar datos
        currentItem = item;
        currentAmount = 1;
        rewardReady = false;

        // cancelar cualquier cola previa
        if (rewardDelayCoroutine != null)
        {
            StopCoroutine(rewardDelayCoroutine);
            rewardDelayCoroutine = null;
        }

        if (rewardCoroutine != null)
        {
            StopCoroutine(rewardCoroutine);
            rewardCoroutine = null;
        }

        // iniciar delay (usa un wait real si useUnscaledTime)
        rewardDelayCoroutine = StartCoroutine(DelayedRewardRoutine(item, slot, type));
    }

    IEnumerator DelayedRewardRoutine(ItemData item, InventorySlot slot, SlotIconType type)
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(rewardDelay);
        else
            yield return new WaitForSeconds(rewardDelay);

        rewardDelayCoroutine = null;

        // elegir ruta de animación
        if (type == SlotIconType.Legendary)
        {
            rewardCoroutine = StartCoroutine(LegendaryRewardRoutine(item, slot, type));
        }
        else
        {
            rewardCoroutine = StartCoroutine(RewardRoutine(item, slot, type));
        }
    }

    // RewardRoutine ya existente (se mantiene, pero nos aseguramos de usar rewardCoroutine)
    IEnumerator RewardRoutine(ItemData item, InventorySlot slot, SlotIconType type)
    {
        rewardPopup.SetActive(true);

        rewardIcon.sprite = item.icon;
        rewardIcon.enabled = true;

        // BACKGROUND
        if (rewardBackground != null)
        {
            switch (type)
            {
                case SlotIconType.Common: rewardBackground.sprite = commonBG; break;
                case SlotIconType.Rare: rewardBackground.sprite = rareBG; break;
                case SlotIconType.Epic: rewardBackground.sprite = epicBG; break;
                case SlotIconType.Legendary: rewardBackground.sprite = legendaryBG; break;
            }
        }

        // GLOW
        if (glow != null)
            glow.color = GetColorByRarity(type);

        // PARTICLES
        if (rewardParticles != null)
        {
            var main = rewardParticles.main;
            Color c = GetColorByRarity(type);
            float intensity = 1f;
            if (type == SlotIconType.Rare) intensity = 1.2f;
            if (type == SlotIconType.Epic) intensity = 1.5f;
            if (type == SlotIconType.Legendary) intensity = 2f;
            main.startColor = c * intensity;
            rewardParticles.Play();
        }

        if ((type == SlotIconType.Epic || type == SlotIconType.Legendary) && uiShake != null)
            uiShake.Play();

        if ((type == SlotIconType.Epic || type == SlotIconType.Legendary) && mainCamera != null)
            StartCoroutine(CameraZoom());

        PlayRewardSound(type);

        // POP (animación sencilla)
        rewardIconRect.anchoredPosition = Vector2.zero;
        rewardIconRect.localScale = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            rewardIconRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * 8f;
            yield return null;
        }

        rewardIconRect.localScale = Vector3.one;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(0.2f);
        else
            yield return new WaitForSeconds(0.2f);

        // MOVE hacia la izquierda (misma lógica anterior)
        Vector2 start = rewardIconRect.anchoredPosition;
        Vector2 target = new Vector2(-200f, 0f);

        t = 0f;
        while (t < 1f)
        {
            rewardIconRect.anchoredPosition = Vector2.Lerp(start, target, t);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / moveDuration;
            yield return null;
        }

        rewardIconRect.anchoredPosition = target;

        if (customTooltip != null)
            customTooltip.Show(item);

        rewardReady = true;
        rewardCoroutine = null;
    }

    // Animación especial para legendary
    IEnumerator LegendaryRewardRoutine(ItemData item, InventorySlot slot, SlotIconType type)
    {
        // Activar popup y contenido
        rewardPopup.SetActive(true);
        rewardIcon.sprite = item.icon;
        rewardIcon.enabled = true;

        // BACKGROUND
        if (rewardBackground != null)
            rewardBackground.sprite = legendaryBG;

        // GLOW intenso
        if (glow != null)
            glow.color = GetColorByRarity(type);

        // Partículas especiales
        if (legendaryParticles != null)
            legendaryParticles.Play();
        else if (rewardParticles != null)
            rewardParticles.Play();

        // Efectos UX fuertes
        if (uiShake != null) uiShake.Play();
        if (mainCamera != null) StartCoroutine(CameraZoom());

        // Sonido legendario con variante
        if (audioSource != null && legendarySFX != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(legendarySFX);
        }
        else
        {
            PlayRewardSound(type);
        }

        // POP sobre‑exagerado -> bounce
        rewardIconRect.anchoredPosition = Vector2.zero;
        rewardIconRect.localScale = Vector3.zero;

        float t = 0f;
        // fase 1: escalar hasta legendaryPopScale (overshoot)
        while (t < 1f)
        {
            float tt = t;
            // easeOutBack-like (simple)
            float s = 1.70158f;
            float overshoot = legendaryPopScale;
            float value = 1f + (overshoot - 1f) * (-Mathf.Pow(2f, -10f * tt) + 1f); // rápida aproximación
            rewardIconRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * legendaryPopScale, value);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / (legendaryPopDuration * 0.6f);
            yield return null;
        }

        // fase 2: rebasado -> volver a 1 con bounce
        t = 0f;
        float dur2 = legendaryPopDuration * 0.4f;
        while (t < 1f)
        {
            // smoothstep down to 1
            float v = Mathf.SmoothStep(legendaryPopScale, 1f, t);
            rewardIconRect.localScale = Vector3.one * v;
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / dur2;
            yield return null;
        }

        rewardIconRect.localScale = Vector3.one;

        // Pequeña pausa dramática
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(0.35f);
        else
            yield return new WaitForSeconds(0.35f);

        // MOVE hacia la izquierda (más lenta/perceptible)
        Vector2 start = rewardIconRect.anchoredPosition;
        Vector2 target = new Vector2(-200f, 0f);

        t = 0f;
        float localMoveDuration = moveDuration * 1.1f;
        while (t < 1f)
        {
            rewardIconRect.anchoredPosition = Vector2.Lerp(start, target, t);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / localMoveDuration;
            yield return null;
        }

        rewardIconRect.anchoredPosition = target;

        if (customTooltip != null)
            customTooltip.Show(item);

        rewardReady = true;
        rewardCoroutine = null;
    }

    // =========================
    public void OnClickReward()
    {
        if (!rewardReady) return;
        StartCoroutine(FlyToPlayer());
    }

    IEnumerator FlyToPlayer()
    {
        HeroController player = FindFirstObjectByType<HeroController>();
        if (player == null) yield break;

        Vector3 start = rewardIconRect.position;
        Vector3 target = player.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            rewardIconRect.position = Vector3.Lerp(start, target, t);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * 3f;
            yield return null;
        }

        bool added = MetaInventory.Instance.AddItem(currentItem, currentAmount);
        if (!added) DropItem(currentItem);

  
        if (customTooltip != null)
            customTooltip.Hide();

        if (rewardPopup != null)
            rewardPopup.SetActive(false);

        yield return null;

        if (currentMachine != null)
            currentMachine.Close();
    }

    void DropItem(ItemData item)
    {
        HeroController player = FindFirstObjectByType<HeroController>();
        if (player == null || item.worldPrefab == null) return;

        Vector3 pos = player.transform.position + Random.insideUnitSphere * 1f;
        Instantiate(item.worldPrefab, pos, Quaternion.identity);
    }

    // =========================
    public void OnCloseButton()
    {
        // detener coroutines de esta UI
        if (rewardCoroutine != null) { StopCoroutine(rewardCoroutine); rewardCoroutine = null; }
        if (stopReelsCoroutine != null) { StopCoroutine(stopReelsCoroutine); stopReelsCoroutine = null; }

        // parar coroutines de spin por reel
        if (reelVisuals != null)
        {
            for (int i = 0; i < reelVisuals.Length; i++)
            {
                if (reelVisuals[i] != null && reelVisuals[i].spinCoroutine != null)
                {
                    StopCoroutine(reelVisuals[i].spinCoroutine);
                    reelVisuals[i].spinCoroutine = null;
                }
            }
        }

        PlaySpinLoop(false);
        if (rewardPopup != null) rewardPopup.SetActive(false);
        if (customTooltip != null)
            customTooltip.Hide();
        FindFirstObjectByType<SlotMachine>()?.Close();
    }

    // =========================
    void PlayButtonSound()
    {
        if (audioSource != null && buttonSFX != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(buttonSFX);
        }
    }

    void PlaySpinLoop(bool play)
    {
        if (audioSource == null || spinLoopSFX == null) return;

        if (play)
        {
            audioSource.clip = spinLoopSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    void PlayRewardSound(SlotIconType type)
    {
        if (audioSource == null) return;

        AudioClip clip = commonSFX;
        if (type == SlotIconType.Rare) clip = rareSFX;
        if (type == SlotIconType.Epic) clip = epicSFX;
        if (type == SlotIconType.Legendary) clip = legendarySFX;

        if (clip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip);
        }
    }

    Color GetColorByRarity(SlotIconType type)
    {
        switch (type)
        {
            case SlotIconType.Common: return Color.white;
            case SlotIconType.Rare: return new Color(0.2f, 0.6f, 1f);
            case SlotIconType.Epic: return new Color(0.6f, 0f, 1f);
            case SlotIconType.Legendary: return new Color(1f, 0.8f, 0f);
        }
        return Color.white;
    }

    IEnumerator CameraZoom()
    {
        if (mainCamera == null) yield break;

        float start = mainCamera.orthographicSize;
        float target = start * zoomAmount;
        float t = 0f;

        while (t < 1f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(start, target, t);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / zoomDuration;
            yield return null;
        }

        mainCamera.orthographicSize = target;

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(0.25f);
        else
            yield return new WaitForSeconds(0.25f);

        t = 0f;
        while (t < 1f)
        {
            mainCamera.orthographicSize = Mathf.Lerp(target, start, t);
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / zoomDuration;
            yield return null;
        }

        mainCamera.orthographicSize = start;
    }


    IEnumerator LossRoutine()
    {
        rewardPopup.SetActive(true);

        explosionImage.gameObject.SetActive(true);
        explosionImage.transform.localScale = Vector3.zero;

        float t = 0f;

        while (t < 1f)
        {
            explosionImage.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 2f, t);
            t += Time.unscaledDeltaTime * 5f;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.3f);

        explosionImage.gameObject.SetActive(false);
        rewardPopup.SetActive(false);

        FindFirstObjectByType<SlotMachine>()?.Close();
    }

    public void ShowLoss()
    {
        StopAllCoroutines();
        StartCoroutine(LossRoutine());
    }

    public void SetMachine(SlotMachine machine)
    {
        currentMachine = machine;
    }
}