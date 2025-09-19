using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class UImodules3D : MonoBehaviour
{
    [Header("Referencias a Objetos 3D (en Canvas) - deben corresponder por índice al inventario")]
    [Tooltip("Arrastrá aquí los GameObjects 3D que renderizás en cada slot (orden: slot 0, slot 1, ...).")]
    public GameObject[] ui3DObjects;

    [Header("Escalado de resaltado")]
    [Tooltip("Multiplicador por defecto para el objeto seleccionado (ej: 1.15 = +15%)")]
    public float defaultScaleMultiplier = 1.15f;
    [Tooltip("Si querés multiplicadores distintos por objeto, pon un array del mismo tamaño que ui3DObjects")]
    public float[] perObjectMultiplier;
    [Tooltip("Duración (segundos) de la animación de escalado")]
    public float scaleDuration = 0.12f;

    [Header("Comportamiento de equip/enable")]
    [Tooltip("Si está ON, se desactivan los objetos al inicio (Awake)")]
    public bool deactivateAtStart = true;
    [Tooltip("Si está ON, al habilitar este componente se activan automáticamente los ui3DObjects")]
    public bool activateOnEnable = true;

    [Header("Highlight automático al Enable")]
    [Tooltip("Si está ON, cuando este componente se habilite hará HighlightObject(highlightIndexOnEnable).")]
    public bool highlightOnEnable = true;
    [Tooltip("Índice a resaltar cuando se habilita (si tu instancia tiene solo un objeto, dejá 0).")]
    public int highlightIndexOnEnable = 0;
    [Tooltip("Delay opcional antes de hacer el highlight (segundos). Útil si hay otras inicializaciones en juego).")]
    public float highlightDelay = 0.05f;

    // Guardados de transform original para restaurar al desactivar
    private Vector3[] originalLocalPos;
    private Quaternion[] originalLocalRot;
    private Vector3[] originalLocalScale;

    // Coroutines por slot
    private Coroutine[] scaleCoroutines;

    // Índice resaltado actualmente (-1 = ninguno)
    private int highlightedIndex = -1;

    void Awake()
    {
        if (ui3DObjects == null || ui3DObjects.Length == 0) return;

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

            if (deactivateAtStart)
                o.SetActive(false);
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
        // Restaurar transform originales inmediatamente (evitar coroutines desde objetos inactivos)
        if (ui3DObjects == null || originalLocalScale == null) return;

        for (int i = 0; i < ui3DObjects.Length; i++)
        {
            if (ui3DObjects[i] == null) continue;
            ui3DObjects[i].transform.localScale = originalLocalScale[i];
            ui3DObjects[i].transform.localPosition = originalLocalPos[i];
            ui3DObjects[i].transform.localRotation = originalLocalRot[i];
        }
        highlightedIndex = -1;
    }

    private IEnumerator DelayedHighlight(int idx, float delay)
    {
        yield return new WaitForSeconds(delay);
        HighlightObject(idx);
    }

    // -------------------- API pública --------------------

    /// <summary>
    /// Activa los primeros 'count' slots (0..count-1) y desactiva el resto.
    /// Útil para sincronizar con un inventario que mantiene el orden por índice.
    /// </summary>
    public void SyncWithCount(int count)
    {
        if (ui3DObjects == null) return;
        for (int i = 0; i < ui3DObjects.Length; i++)
        {
            if (ui3DObjects[i] == null) continue;
            bool shouldBeActive = i < count;
            ui3DObjects[i].SetActive(shouldBeActive);
            if (shouldBeActive)
            {
                // aseguramos transform original al activar
                ui3DObjects[i].transform.localScale = originalLocalScale[i];
                ui3DObjects[i].transform.localPosition = originalLocalPos[i];
                ui3DObjects[i].transform.localRotation = originalLocalRot[i];
            }
        }
    }

    /// <summary>
    /// Sincroniza con tu clase Inventory (debe exponer MyItemsCount() -> int).
    /// </summary>
    public void SyncWithInventory(Inventory inv)
    {
        if (inv == null || ui3DObjects == null) return;

        // Revisamos todos los slots visibles en UI (ui3DObjects)
        for (int i = 0; i < ui3DObjects.Length; i++)
        {
            if (ui3DObjects[i] == null) continue;

            bool shouldBeActive = false;

            // Si el inventario tiene al menos 'i+1' items, consultamos el módulo en esa posición
            if (i < inv.MyItemsCount())
            {
                // Usamos GetModuleAtIndex (tu Player/Drops usan este método)
                GameObject module = inv.GetModuleAtIndex(i);
                shouldBeActive = (module != null);
            }

            ui3DObjects[i].SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                // Restaurar transform original para que no quede "pegado" o escalado raro
                ui3DObjects[i].transform.localScale = originalLocalScale[i];
                ui3DObjects[i].transform.localPosition = originalLocalPos[i];
                ui3DObjects[i].transform.localRotation = originalLocalRot[i];
            }
        }
    }


    /// <summary>
    /// Activa el objeto (equipa) en el índice dado.
    /// </summary>
    public void EquipObject(int index)
    {
        if (!IsValidIndex(index)) return;
        var o = ui3DObjects[index];
        o.SetActive(true);
        o.transform.localScale = originalLocalScale[index];
        o.transform.localPosition = originalLocalPos[index];
        o.transform.localRotation = originalLocalRot[index];
    }

    /// <summary>
    /// Resalta (agranda) el objeto por índice; el anterior resaltado vuelve a su escala original.
    /// Si index == -1 desresalta todo.
    /// </summary>
    public void HighlightObject(int index)
    {
        if (index == highlightedIndex) return;

        // devolver anterior a escala original
        if (IsValidIndex(highlightedIndex) && ui3DObjects[highlightedIndex] != null)
        {
            StartScaleToOriginal(highlightedIndex);
        }

        highlightedIndex = -1;

        // si index válido, animar nuevo highlight
        if (IsValidIndex(index) && ui3DObjects[index] != null)
        {
            if (!ui3DObjects[index].activeInHierarchy) ui3DObjects[index].SetActive(true);

            float mul = GetMultiplierForIndex(index);
            Vector3 target = originalLocalScale[index] * mul;
            StartScaleCoroutine(index, ui3DObjects[index].transform.localScale, target, scaleDuration);

            highlightedIndex = index;
        }
    }

    /// <summary> Desresalta el actual (vuelve a escala original) </summary>
    public void UnhighlightCurrent()
    {
        if (IsValidIndex(highlightedIndex))
        {
            StartScaleToOriginal(highlightedIndex);
            highlightedIndex = -1;
        }
    }

    /// <summary> Desresalta todos (animado) </summary>
    public void UnhighlightAll()
    {
        if (ui3DObjects == null) return;
        for (int i = 0; i < ui3DObjects.Length; i++)
        {
            if (ui3DObjects[i] == null) continue;
            StartScaleToOriginal(i);
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

        // Si este componente o su gameObject NO están activos, no arrancamos coroutine:
        // aplicamos la escala directamente (evitamos error "Coroutine couldn't be started...").
        if (!this.isActiveAndEnabled || !this.gameObject.activeInHierarchy)
        {
            if (ui3DObjects[index] != null)
                ui3DObjects[index].transform.localScale = to;
            scaleCoroutines[index] = null;
            return;
        }

        // parar coroutine previa si existe
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
            float sp = p * p * (3f - 2f * p); // smoothstep-like
            t.localScale = Vector3.LerpUnclamped(from, to, sp);
            yield return null;
        }
        t.localScale = to;
        scaleCoroutines[index] = null;
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
