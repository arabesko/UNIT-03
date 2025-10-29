using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterTitleController : MonoBehaviour
{
    [Header("Panels (arrastrá tus paneles de Título aquí)")]
    public List<GameObject> titlePanels = new List<GameObject>();

    [Header("Audio Clips")]
    public AudioClip enterClip;
    public AudioClip loopClip;
    public AudioClip exitClip;

    [Header("Timing")]
    [Tooltip("Segundos que dura el fade-in (y fade-out).")]
    public float appearDuration = 0.8f;
    [Tooltip("Tiempo por defecto que el título permanece en pantalla.")]
    public float defaultDisplayDuration = 3.0f;

    [Header("Delay")]
    [Tooltip("Delay por defecto antes de que empiece la primera aparición.")]
    public float defaultDelay = 2.0f;
    [Tooltip("Delay entre paneles si reproducís la secuencia completa.")]
    public float delayBetweenPanels = 1.0f;

    [Header("Glitch parameters")]
    public float maxJitter = 8f;
    public float enterGlitchRate = 25f;
    public float visibleGlitchRate = 3f;
    public float exitGlitchRate = 40f;

    [Header("Auto show (start) - para testing / behavior")]
    [Tooltip("Si está ON, al iniciar la escena el script se autoejecuta.")]
    public bool autoShowInPlay = true;            // por defecto true según lo pediste
    [Tooltip("Si está ON muestra TODOS los panels en secuencia; si OFF muestra solo el panel de autoShowIndex.")]
    public bool autoPlaySequence = true;          // por defecto mostramos toda la lista
    [Tooltip("Índice del panel a mostrar si autoPlaySequence = false")]
    public int autoShowIndex = 0;

    [Header("Debug")]
    [Tooltip("Pruebas: presioná P en Play para repetir el último título mostrado.")]
    public bool enableReplayWithP = true;

    // internals
    public AudioSource _audioSource;
    private Coroutine _activeDisplayCoroutine;
    private Coroutine _delayCoroutine;
    private Dictionary<GameObject, CanvasGroup> _cgCache = new Dictionary<GameObject, CanvasGroup>();

    // track last shown for replay
    private GameObject _lastShownPanel = null;
    private float _lastDisplayTime = 0f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // aseguramos panels apagados y con CanvasGroup
        foreach (var p in titlePanels)
        {
            if (p == null) continue;
            var cg = GetOrAddCanvasGroup(p);
            cg.alpha = 0f;
            p.SetActive(false);
        }

        // auto show behavior
        if (autoShowInPlay)
        {
            if (autoPlaySequence)
            {
                StartCoroutine(AutoShowSequenceRoutine());
            }
            else
            {
                int idx = Mathf.Clamp(autoShowIndex, 0, Mathf.Max(0, titlePanels.Count - 1));
                StartCoroutine(AutoShowSingleRoutine(idx));
            }
        }
    }

    private void Update()
    {
        if (enableReplayWithP && Input.GetKeyDown(KeyCode.P))
        {
            if (_lastShownPanel != null)
            {
                Debug.Log("[ChapterTitleController] Replay (P) - repitiendo último título.");
                ShowPanel(_lastShownPanel, _lastDisplayTime);
            }
            else
            {
                Debug.Log("[ChapterTitleController] Replay (P) pulsada pero no hay título previo.");
            }
        }
    }

    // ---------------- Auto show coroutines ----------------

    private IEnumerator AutoShowSequenceRoutine()
    {
        if (titlePanels == null || titlePanels.Count == 0)
        {
            Debug.LogWarning("[ChapterTitleController] AutoShowSequence: no hay panels en la lista.");
            yield break;
        }

        Debug.Log($"[ChapterTitleController] AutoShowSequence: esperando defaultDelay {defaultDelay}s");
        yield return new WaitForSeconds(Mathf.Max(0f, defaultDelay));

        for (int i = 0; i < titlePanels.Count; i++)
        {
            var panel = titlePanels[i];
            if (panel == null) continue;

            Debug.Log($"[ChapterTitleController] AutoShowSequence: mostrando panel {i} -> {panel.name}");
            ShowPanel(panel, defaultDisplayDuration);

            // esperamos hasta que termine su reproducción
            float totalTime = defaultDisplayDuration + (2f * appearDuration);
            // además agregar un pequeño gap entre panels
            yield return new WaitForSeconds(totalTime + Mathf.Max(0f, delayBetweenPanels));
        }
    }

    private IEnumerator AutoShowSingleRoutine(int index)
    {
        if (titlePanels == null || titlePanels.Count == 0)
        {
            Debug.LogWarning("[ChapterTitleController] AutoShowSingle: no hay panels en la lista.");
            yield break;
        }

        index = Mathf.Clamp(index, 0, titlePanels.Count - 1);
        Debug.Log($"[ChapterTitleController] AutoShowSingle: esperando defaultDelay {defaultDelay}s para index {index}");
        yield return new WaitForSeconds(Mathf.Max(0f, defaultDelay));
        ShowPanel(titlePanels[index], defaultDisplayDuration);
    }

    // ---------------- Public API ----------------

    public void ShowPanel(GameObject panel, float displayTime)
    {
        if (panel == null) { Debug.LogWarning("[ChapterTitleController] ShowPanel: panel null"); return; }
        float t = displayTime > 0f ? displayTime : defaultDisplayDuration;
        Debug.Log($"[ChapterTitleController] ShowPanel -> {panel.name} por {t}s");

        // guardar para replay
        _lastShownPanel = panel;
        _lastDisplayTime = t;

        if (_delayCoroutine != null) { StopCoroutine(_delayCoroutine); _delayCoroutine = null; }
        if (_activeDisplayCoroutine != null) { StopCoroutine(_activeDisplayCoroutine); _activeDisplayCoroutine = null; }

        TitleModel model = new TitleModel(panel, t);
        TitlePlayer player = new TitlePlayer(this, _audioSource);
        _activeDisplayCoroutine = StartCoroutine(RunPlayer(player, model));
    }

    public void ShowPanelByIndex(int index, float displayTime = -1f)
    {
        if (index < 0 || index >= titlePanels.Count) { Debug.LogWarning("[ChapterTitleController] Index fuera de rango"); return; }
        ShowPanel(titlePanels[index], displayTime);
    }

    public void ShowPanelDelayed(GameObject panel, float delaySeconds = -1f, float displayTime = -1f)
    {
        if (panel == null) { Debug.LogWarning("[ChapterTitleController] ShowPanelDelayed: panel null"); return; }
        float delayToUse = (delaySeconds <= 0f) ? defaultDelay : delaySeconds;
        float dispToUse = (displayTime > 0f) ? displayTime : defaultDisplayDuration;

        if (_delayCoroutine != null) StopCoroutine(_delayCoroutine);
        _delayCoroutine = StartCoroutine(DelayedShowRoutine(panel, delayToUse, dispToUse));
        Debug.Log($"[ChapterTitleController] ShowPanelDelayed pedido para {panel.name} en {delayToUse}s (display {dispToUse}s)");
    }

    public void ShowPanelByIndexDelayed(int index, float delaySeconds = -1f, float displayTime = -1f)
    {
        if (index < 0 || index >= titlePanels.Count) { Debug.LogWarning("[ChapterTitleController] IndexDelayed fuera de rango"); return; }
        ShowPanelDelayed(titlePanels[index], delaySeconds, displayTime);
    }

    // -------------- Implementation ----------------

    private IEnumerator DelayedShowRoutine(GameObject panel, float delaySeconds, float displayTime)
    {
        Debug.Log($"[ChapterTitleController] DelayedShowRoutine esperando {delaySeconds}s para {panel.name}");
        yield return new WaitForSeconds(delaySeconds);
        _delayCoroutine = null;
        ShowPanel(panel, displayTime);
    }

    private IEnumerator RunPlayer(TitlePlayer player, TitleModel model)
    {
        yield return StartCoroutine(player.Play(model));
        _activeDisplayCoroutine = null;
    }

    // ---------- Sequences + glitch ----------

    private IEnumerator EnterSequence(CanvasGroup cg, TitleModel model)
    {
        float elapsed = 0f;
        if (cg != null) cg.alpha = 0f;
        Vector2 basePos = GetRectPosition(model.panel);

        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / appearDuration);
            if (cg != null) cg.alpha = t;
            DoGlitchStep(model.panel, enterGlitchRate);
            SetRectPosition(model.panel, basePos + RandomJitter(maxJitter * (1f - t)));
            yield return null;
        }

        if (cg != null) cg.alpha = 1f;
        SetRectPosition(model.panel, basePos);
    }

    private IEnumerator VisibleSequence(CanvasGroup cg, TitleModel model)
    {
        float remaining = model.displayTime;
        Vector2 basePos = GetRectPosition(model.panel);

        while (remaining > 0f)
        {
            DoGlitchStep(model.panel, visibleGlitchRate);
            SetRectPosition(model.panel, basePos + RandomJitter(maxJitter * 0.25f));
            remaining -= 0.15f;
            yield return new WaitForSeconds(0.15f);
        }

        SetRectPosition(model.panel, basePos);
    }

    private IEnumerator ExitSequence(CanvasGroup cg, TitleModel model)
    {
        float elapsed = 0f;
        Vector2 basePos = GetRectPosition(model.panel);

        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / appearDuration);
            if (cg != null) cg.alpha = 1f - t;
            DoGlitchStep(model.panel, exitGlitchRate);
            SetRectPosition(model.panel, basePos + RandomJitter(maxJitter * (1f - t + 0.2f)));
            yield return null;
        }

        if (cg != null) cg.alpha = 0f;
        SetRectPosition(model.panel, basePos);
    }

    private void DoGlitchStep(GameObject panel, float rate)
    {
        if (panel == null) return;
        float prob = Mathf.Clamp01(rate * Time.deltaTime);
        if (Random.value < prob)
        {
            CanvasGroup cg = GetOrAddCanvasGroup(panel);
            StartCoroutine(ShortFlash(cg, Random.Range(0.03f, 0.08f)));
            RectTransform rt = panel.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 jitter = RandomJitter(maxJitter);
                SetRectPosition(panel, GetRectPosition(panel) + jitter);
                StartCoroutine(ResetPositionAfterDelay(panel, 0.06f));
            }
        }
    }

    private IEnumerator ShortFlash(CanvasGroup cg, float dur)
    {
        if (cg == null) yield break;
        float original = cg.alpha;
        cg.alpha = Mathf.Clamp01(original * Random.Range(0.2f, 0.6f));
        yield return new WaitForSeconds(dur);
        cg.alpha = original;
    }

    private IEnumerator ResetPositionAfterDelay(GameObject panel, float delay)
    {
        if (panel == null) yield break;
        Vector2 basePos = GetRectPosition(panel);
        yield return new WaitForSeconds(delay);
        SetRectPosition(panel, basePos);
    }

    // ---------- Utilities ----------

    private CanvasGroup GetOrAddCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        if (_cgCache.ContainsKey(panel)) return _cgCache[panel];
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        _cgCache[panel] = cg;
        return cg;
    }

    private Vector2 GetRectPosition(GameObject panel)
    {
        if (panel == null) return Vector2.zero;
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null) return rt.anchoredPosition;
        Vector3 lp = panel.transform.localPosition;
        return new Vector2(lp.x, lp.y);
    }

    private void SetRectPosition(GameObject panel, Vector2 pos)
    {
        if (panel == null) return;
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null) { rt.anchoredPosition = pos; return; }
        var v3 = panel.transform.localPosition;
        panel.transform.localPosition = new Vector3(pos.x, pos.y, v3.z);
    }

    private Vector2 RandomJitter(float magnitude)
    {
        return new Vector2(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude));
    }

    [ContextMenu("Play first panel (debug)")]
    private void DebugPlayFirst()
    {
        if (titlePanels.Count > 0) ShowPanel(titlePanels[0], defaultDisplayDuration);
    }

    // ---------- Inner MVC-ish types ----------
    private class TitleModel
    {
        public GameObject panel;
        public float displayTime;
        public TitleModel(GameObject p, float t) { panel = p; displayTime = t; }
    }

    private class TitlePlayer
    {
        private ChapterTitleController _owner;
        private AudioSource _audioSource;

        public TitlePlayer(ChapterTitleController owner, AudioSource audioSource)
        {
            _owner = owner;
            _audioSource = audioSource;
        }

        public IEnumerator Play(TitleModel model)
        {
            if (model == null || model.panel == null) yield break;
            CanvasGroup cg = _owner.GetOrAddCanvasGroup(model.panel);
            model.panel.SetActive(true);

            if (_owner.enterClip != null) _audioSource.PlayOneShot(_owner.enterClip);
            yield return _owner.EnterSequence(cg, model);

            if (_owner.loopClip != null)
            {
                _audioSource.clip = _owner.loopClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }

            yield return _owner.VisibleSequence(cg, model);

            if (_owner.loopClip != null)
            {
                _audioSource.loop = false;
                _audioSource.Stop();
            }

            if (_owner.exitClip != null) _audioSource.PlayOneShot(_owner.exitClip);
            yield return _owner.ExitSequence(cg, model);

            model.panel.SetActive(false);
        }
    }
}