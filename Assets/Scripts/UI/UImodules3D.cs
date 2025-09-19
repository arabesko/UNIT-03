using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UImodules3D : MonoBehaviour
{
    [Header("Referencias a Objetos 3D (en Canvas) - deben corresponder por índice al inventario")]
    public GameObject[] ui3DObjects;

    [Header("Backgrounds (opcional)")]
    [Tooltip("Recuadros de fondo a resaltar. Deben corresponder por índice con ui3DObjects (pueden ser UI Image o cualquier GameObject).")]
    public GameObject[] uiBackgroundObjects;

    [Header("Bordes luminosos (opcional)")]
    [Tooltip("Bordes luminosos para resaltar. Deben corresponder por índice con ui3DObjects.")]
    public GameObject[] highlightBorders;

    [Header("Escalado de resaltado")]
    [Tooltip("Multiplicador por defecto para el objeto seleccionado (ej: 1.15 = +15%)")]
    public float defaultScaleMultiplier = 1.15f;
    [Tooltip("Si quieres multiplicadores distintos por objeto, pon un array del mismo tamaño que ui3DObjects")]
    public float[] perObjectMultiplier;
    [Tooltip("Duración (segundos) de la animación de escalado")]
    public float scaleDuration = 0.12f;

    [Header("Escalado del background (adicional)")]
    [Tooltip("Cuánto escala el recuadro de fondo cuando se resalta (multiplicador).")]
    public float backgroundScaleMultiplier = 1.08f;

    [Header("Color del background (opcional si el background tiene Image)")]
    [Tooltip("Si está ON, el script también tintará la Image del background al resaltar.")]
    public bool highlightBackgroundColor = true;
    public Color backgroundHighlightColor = new Color(0.15f, 0.9f, 1f, 1f);
    [Tooltip("Duración de la animación de color (se usa la misma que scaleDuration si está en 0)")]
    public float backgroundColorDuration = 0.12f;

    [Header("Comportamiento de equip/enable")]
    public bool deactivateAtStart = true;
    public bool activateOnEnable = true;

    [Header("Highlight automático al Enable")]
    public bool highlightOnEnable = true;
    public int highlightIndexOnEnable = 0;
    public float highlightDelay = 0.05f;

    // Guardados de transform original para restaurar al desactivar
    private Vector3[] originalLocalPos;
    private Quaternion[] originalLocalRot;
    private Vector3[] originalLocalScale;

    // Background originals
    private Vector3[] originalBackgroundScale;
    private Image[] backgroundImages; // si el background es UI.Image
    private Color[] originalBackgroundColors;

    // Coroutines por slot
    private Coroutine[] scaleCoroutines;
    private Coroutine[] bgScaleCoroutines;
    private Coroutine[] bgColorCoroutines;

    // Índice resaltado actualmente (-1 = ninguno)
    private int highlightedIndex = -1;

    void Awake()
    {
        // inicializar arrays ui3DObjects
        if (ui3DObjects != null && ui3DObjects.Length > 0)
        {
            int c = ui3DObjects.Length;
            originalLocalPos = new Vector3[c];
            originalLocalRot = new Quaternion[c];
            originalLocalScale = new Vector3[c];
            scaleCoroutines = new Coroutine[c];

            for (int i = 0; i < c; i++)
            {
                var o = ui3DObjects[i];
                if (o == null) continue;
                originalLocalPos[i] = o.transform.localPosition;
                originalLocalRot[i] = o.transform.localRotation;
                originalLocalScale[i] = o.transform.localScale;
                if (deactivateAtStart) o.SetActive(false);
            }
        }

        // inicializar arrays backgrounds - NO desactivar al inicio
        if (uiBackgroundObjects != null && uiBackgroundObjects.Length > 0)
        {
            int c = uiBackgroundObjects.Length;
            originalBackgroundScale = new Vector3[c];
            bgScaleCoroutines = new Coroutine[c];
            bgColorCoroutines = new Coroutine[c];
            backgroundImages = new Image[c];
            originalBackgroundColors = new Color[c];

            for (int i = 0; i < c; i++)
            {
                var b = uiBackgroundObjects[i];
                if (b == null) continue;
                originalBackgroundScale[i] = b.transform.localScale;

                var img = b.GetComponent<Image>();
                backgroundImages[i] = img;
                if (img != null)
                    originalBackgroundColors[i] = img.color;

                // Eliminar desactivación inicial:
                // if (deactivateAtStart) b.SetActive(false);
            }
        }

        // inicializar bordes luminosos - desactivar al inicio
        if (highlightBorders != null && highlightBorders.Length > 0)
        {
            for (int i = 0; i < highlightBorders.Length; i++)
            {
                if (highlightBorders[i] == null) continue;
                highlightBorders[i].SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        if (activateOnEnable && ui3DObjects != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                var o = ui3DObjects[i];
                if (o == null) continue;
                o.SetActive(true);
                o.transform.localPosition = originalLocalPos[i];
                o.transform.localRotation = originalLocalRot[i];
                o.transform.localScale = originalLocalScale[i];
            }
        }

        // Activar siempre los backgrounds
        if (uiBackgroundObjects != null)
        {
            for (int i = 0; i < uiBackgroundObjects.Length; i++)
            {
                var b = uiBackgroundObjects[i];
                if (b == null) continue;
                b.SetActive(true); // Siempre activos
                b.transform.localScale = originalBackgroundScale[i];
                if (backgroundImages[i] != null)
                    backgroundImages[i].color = originalBackgroundColors[i];
            }
        }

        // Desactivar todos los bordes al inicio
        if (highlightBorders != null)
        {
            for (int i = 0; i < highlightBorders.Length; i++)
            {
                if (highlightBorders[i] == null) continue;
                highlightBorders[i].SetActive(false);
            }
        }

        if (highlightOnEnable && IsValidIndex(highlightIndexOnEnable))
        {
            if (highlightDelay > 0f)
                StartCoroutine(DelayedHighlight(highlightIndexOnEnable, highlightDelay));
            else
                HighlightObject(highlightIndexOnEnable);
        }
    }

    void OnDisable()
    {
        // Restaurar transform originales inmediatamente
        if (ui3DObjects != null && originalLocalScale != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                if (ui3DObjects[i] == null) continue;
                ui3DObjects[i].transform.localScale = originalLocalScale[i];
                ui3DObjects[i].transform.localPosition = originalLocalPos[i];
                ui3DObjects[i].transform.localRotation = originalLocalRot[i];
            }
        }

        if (uiBackgroundObjects != null && originalBackgroundScale != null)
        {
            for (int i = 0; i < uiBackgroundObjects.Length; i++)
            {
                if (uiBackgroundObjects[i] == null) continue;
                uiBackgroundObjects[i].transform.localScale = originalBackgroundScale[i];
                if (backgroundImages != null && backgroundImages[i] != null)
                    backgroundImages[i].color = originalBackgroundColors[i];
            }
        }

        // Desactivar todos los bordes
        if (highlightBorders != null)
        {
            for (int i = 0; i < highlightBorders.Length; i++)
            {
                if (highlightBorders[i] == null) continue;
                highlightBorders[i].SetActive(false);
            }
        }

        highlightedIndex = -1;
    }

    private IEnumerator DelayedHighlight(int idx, float delay)
    {
        yield return new WaitForSeconds(delay);
        HighlightObject(idx);
    }

    // -------------------- API pública --------------------

    public void SyncWithCount(int count)
    {
        if (ui3DObjects != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                if (ui3DObjects[i] == null) continue;
                bool shouldBeActive = i < count;
                ui3DObjects[i].SetActive(shouldBeActive);
                if (shouldBeActive)
                {
                    ui3DObjects[i].transform.localScale = originalLocalScale[i];
                    ui3DObjects[i].transform.localPosition = originalLocalPos[i];
                    ui3DObjects[i].transform.localRotation = originalLocalRot[i];
                }
            }
        }

        // Backgrounds siempre visibles - eliminar lógica de desactivación
        if (uiBackgroundObjects != null)
        {
            for (int i = 0; i < uiBackgroundObjects.Length; i++)
            {
                if (uiBackgroundObjects[i] == null) continue;
                // Solo restaurar propiedades, no cambiar activeState
                uiBackgroundObjects[i].transform.localScale = originalBackgroundScale[i];
                if (backgroundImages != null && backgroundImages[i] != null)
                    backgroundImages[i].color = originalBackgroundColors[i];
            }
        }

        // Desactivar bordes si no hay ítem
        if (highlightBorders != null)
        {
            for (int i = 0; i < highlightBorders.Length; i++)
            {
                if (highlightBorders[i] == null) continue;
                bool shouldBeActive = i < count && i == highlightedIndex;
                highlightBorders[i].SetActive(shouldBeActive);
            }
        }
    }

    public void SyncWithInventory(Inventory inv)
    {
        if (inv == null) return;

        // Si tenés GetModuleAtIndex en Inventory, sería mejor (activamos solo los slots con module != null)
        if (ui3DObjects != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                if (ui3DObjects[i] == null) continue;
                bool shouldBeActive = false;
                if (i < inv.MyItemsCount())
                {
                    GameObject module = inv.GetModuleAtIndex(i);
                    shouldBeActive = (module != null);
                }
                ui3DObjects[i].SetActive(shouldBeActive);
                if (shouldBeActive)
                {
                    ui3DObjects[i].transform.localScale = originalLocalScale[i];
                }
            }
        }

        // Backgrounds siempre visibles
        if (uiBackgroundObjects != null)
        {
            for (int i = 0; i < uiBackgroundObjects.Length; i++)
            {
                if (uiBackgroundObjects[i] == null) continue;
                // Solo restaurar propiedades
                uiBackgroundObjects[i].transform.localScale = originalBackgroundScale[i];
                if (backgroundImages != null && backgroundImages[i] != null)
                    backgroundImages[i].color = originalBackgroundColors[i];
            }
        }

        // Desactivar bordes si no hay ítem
        if (highlightBorders != null)
        {
            for (int i = 0; i < highlightBorders.Length; i++)
            {
                if (highlightBorders[i] == null) continue;
                bool shouldBeActive = false;
                if (i < inv.MyItemsCount())
                {
                    GameObject module = inv.GetModuleAtIndex(i);
                    shouldBeActive = (module != null) && i == highlightedIndex;
                }
                highlightBorders[i].SetActive(shouldBeActive);
            }
        }
    }

    public void EquipObject(int index)
    {
        if (!IsValidIndex(index)) return;
        var o = ui3DObjects[index];
        o.SetActive(true);
        o.transform.localScale = originalLocalScale[index];
        o.transform.localPosition = originalLocalPos[index];
        o.transform.localRotation = originalLocalRot[index];

        // Solo restaurar propiedades del background
        if (uiBackgroundObjects != null && index < uiBackgroundObjects.Length && uiBackgroundObjects[index] != null)
        {
            uiBackgroundObjects[index].transform.localScale = originalBackgroundScale[index];
            if (backgroundImages != null && backgroundImages[index] != null)
                backgroundImages[index].color = originalBackgroundColors[index];
        }

        // No activar borde aquí, solo en HighlightObject
    }

    public void HighlightObject(int index)
    {
        if (index == highlightedIndex) return;

        // devolver anterior a escala original
        if (IsValidIndex(highlightedIndex) && ui3DObjects[highlightedIndex] != null)
        {
            StartScaleToOriginal(highlightedIndex);
            StartBgScaleToOriginal(highlightedIndex);
            StartBgColorToOriginal(highlightedIndex);

            // Desactivar borde del item anterior
            if (highlightBorders != null && highlightedIndex < highlightBorders.Length && highlightBorders[highlightedIndex] != null)
            {
                highlightBorders[highlightedIndex].SetActive(false);
            }
        }

        highlightedIndex = -1;

        if (IsValidIndex(index) && ui3DObjects[index] != null)
        {
            if (!ui3DObjects[index].activeInHierarchy) ui3DObjects[index].SetActive(true);

            float mul = GetMultiplierForIndex(index);
            Vector3 target = originalLocalScale[index] * mul;
            StartScaleCoroutine(index, ui3DObjects[index].transform.localScale, target, scaleDuration);

            // background scale
            if (uiBackgroundObjects != null && index < uiBackgroundObjects.Length && uiBackgroundObjects[index] != null)
            {
                if (!uiBackgroundObjects[index].activeInHierarchy) uiBackgroundObjects[index].SetActive(true);
                Vector3 bgTarget = originalBackgroundScale[index] * backgroundScaleMultiplier * mul;
                StartBgScaleCoroutine(index, uiBackgroundObjects[index].transform.localScale, bgTarget, scaleDuration);

                // color
                if (highlightBackgroundColor && backgroundImages != null && backgroundImages[index] != null)
                {
                    StartBgColorCoroutine(index, backgroundImages[index], backgroundImages[index].color, backgroundHighlightColor, (backgroundColorDuration > 0f ? backgroundColorDuration : scaleDuration));
                }
            }

            // Activar borde luminoso del item seleccionado
            if (highlightBorders != null && index < highlightBorders.Length && highlightBorders[index] != null)
            {
                highlightBorders[index].SetActive(true);
            }

            highlightedIndex = index;
        }
    }

    public void UnhighlightCurrent()
    {
        if (IsValidIndex(highlightedIndex))
        {
            StartScaleToOriginal(highlightedIndex);
            StartBgScaleToOriginal(highlightedIndex);
            StartBgColorToOriginal(highlightedIndex);

            // Desactivar borde del item actual
            if (highlightBorders != null && highlightedIndex < highlightBorders.Length && highlightBorders[highlightedIndex] != null)
            {
                highlightBorders[highlightedIndex].SetActive(false);
            }

            highlightedIndex = -1;
        }
    }

    public void UnhighlightAll()
    {
        if (ui3DObjects != null)
        {
            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                if (ui3DObjects[i] == null) continue;
                StartScaleToOriginal(i);
            }
        }
        if (uiBackgroundObjects != null)
        {
            for (int i = 0; i < uiBackgroundObjects.Length; i++)
            {
                if (uiBackgroundObjects[i] == null) continue;
                StartBgScaleToOriginal(i);
                StartBgColorToOriginal(i);
            }
        }

        // Desactivar todos los bordes
        if (highlightBorders != null)
        {
            for (int i = 0; i < highlightBorders.Length; i++)
            {
                if (highlightBorders[i] == null) continue;
                highlightBorders[i].SetActive(false);
            }
        }

        highlightedIndex = -1;
    }

    // -------------------- animación de escalado (internos) --------------------

    private void StartScaleToOriginal(int index)
    {
        if (!IsValidIndex(index) || ui3DObjects[index] == null) return;
        Vector3 from = ui3DObjects[index].transform.localScale;
        Vector3 to = originalLocalScale[index];
        StartScaleCoroutine(index, from, to, scaleDuration);
    }

    private void StartScaleCoroutine(int index, Vector3 from, Vector3 to, float duration)
    {
        if (!IsValidIndex(index)) return;

        if (!this.isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            if (ui3DObjects[index] != null) ui3DObjects[index].transform.localScale = to;
            scaleCoroutines[index] = null;
            return;
        }

        if (scaleCoroutines[index] != null)
        {
            StopCoroutine(scaleCoroutines[index]);
            scaleCoroutines[index] = null;
        }
        scaleCoroutines[index] = StartCoroutine(LerpScale(ui3DObjects[index].transform, from, to, duration, index));
    }

    private IEnumerator LerpScale(Transform t, Vector3 from, Vector3 to, float duration, int index)
    {
        if (t == null) yield break;
        if (duration <= 0f)
        {
            t.localScale = to;
            scaleCoroutines[index] = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float sp = p * p * (3f - 2f * p);
            t.localScale = Vector3.LerpUnclamped(from, to, sp);
            yield return null;
        }
        t.localScale = to;
        scaleCoroutines[index] = null;
    }

    // -------------------- background scale/color coroutines --------------------

    private void StartBgScaleToOriginal(int index)
    {
        if (uiBackgroundObjects == null || index < 0 || index >= uiBackgroundObjects.Length) return;
        if (uiBackgroundObjects[index] == null) return;
        Vector3 from = uiBackgroundObjects[index].transform.localScale;
        Vector3 to = originalBackgroundScale[index];
        StartBgScaleCoroutine(index, from, to, scaleDuration);
    }

    private void StartBgScaleCoroutine(int index, Vector3 from, Vector3 to, float duration)
    {
        if (uiBackgroundObjects == null || index < 0 || index >= uiBackgroundObjects.Length) return;

        if (!this.isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            if (uiBackgroundObjects[index] != null) uiBackgroundObjects[index].transform.localScale = to;
            if (bgScaleCoroutines != null) bgScaleCoroutines[index] = null;
            return;
        }

        if (bgScaleCoroutines[index] != null)
        {
            StopCoroutine(bgScaleCoroutines[index]);
            bgScaleCoroutines[index] = null;
        }
        bgScaleCoroutines[index] = StartCoroutine(LerpScale(uiBackgroundObjects[index].transform, from, to, duration, index));
    }

    private void StartBgColorCoroutine(int index, Image img, Color from, Color to, float duration)
    {
        if (!highlightBackgroundColor || img == null) return;

        if (!this.isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            img.color = to;
            if (bgColorCoroutines != null) bgColorCoroutines[index] = null;
            return;
        }

        if (bgColorCoroutines[index] != null)
        {
            StopCoroutine(bgColorCoroutines[index]);
            bgColorCoroutines[index] = null;
        }
        bgColorCoroutines[index] = StartCoroutine(LerpColor(img, from, to, duration, index));
    }

    private IEnumerator LerpColor(Image img, Color from, Color to, float duration, int index)
    {
        if (img == null) yield break;
        if (duration <= 0f)
        {
            img.color = to;
            bgColorCoroutines[index] = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float sp = t * t * (3f - 2f * t);
            img.color = Color.LerpUnclamped(from, to, sp);
            yield return null;
        }
        img.color = to;
        bgColorCoroutines[index] = null;
    }

    private void StartBgColorToOriginal(int index)
    {
        if (backgroundImages == null || index < 0 || index >= backgroundImages.Length) return;
        var img = backgroundImages[index];
        if (img == null) return;
        StartBgColorCoroutine(index, img, img.color, originalBackgroundColors[index], (backgroundColorDuration > 0f ? backgroundColorDuration : scaleDuration));
    }

    // -------------------- helpers --------------------

    private float GetMultiplierForIndex(int index)
    {
        if (perObjectMultiplier != null && perObjectMultiplier.Length == ui3DObjects.Length)
            return Mathf.Max(0.01f, perObjectMultiplier[index]);
        return defaultScaleMultiplier;
    }

    private bool IsValidIndex(int idx) =>
        ui3DObjects != null && idx >= 0 && idx < ui3DObjects.Length && ui3DObjects[idx] != null;

#if UNITY_EDITOR
    [ContextMenu("UnhighlightAll")]
    private void Editor_UnhighlightAll() => UnhighlightAll();
#endif
}