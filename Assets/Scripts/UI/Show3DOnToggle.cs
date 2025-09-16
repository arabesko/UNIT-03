using System.Collections;
using UnityEngine;

public class Show3DOnToggle : MonoBehaviour
{
    [Header("Referencias a Objetos 3D (en Canvas) - deben corresponder por índice al inventario")]
    public GameObject[] ui3DObjects;

    [Header("Escalado de resaltado")]
    [Tooltip("Multiplicador por defecto para el objeto seleccionado (ej: 1.15 = +15%)")]
    public float defaultScaleMultiplier = 1.15f;
    [Tooltip("Si quieres multiplicadores distintos por objeto, pon un array del mismo tamaño que ui3DObjects")]
    public float[] perObjectMultiplier;
    [Tooltip("Duración (segundos) de la animación de escalado")]
    public float scaleDuration = 0.12f;

    [Header("Comportamiento de equip/enable")]
    [Tooltip("Si está ON, se desactivan los objetos al inicio (Awake)")]
    public bool deactivateAtStart = true;
    [Tooltip("Si está ON, al activarse el componente se activan automáticamente los ui3DObjects")]
    public bool activateOnEnable = true;

    // estado guardado
    private Vector3[] originalLocalPos;
    private Quaternion[] originalLocalRot;
    private Vector3[] originalLocalScale;

    // control de coroutines por objeto
    private Coroutine[] scaleCoroutines;

    // índice actualmente resaltado (-1 ninguno)
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
            if (deactivateAtStart) o.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (!activateOnEnable) return;
        if (ui3DObjects == null) return;
        for (int i = 0; i < ui3DObjects.Length; i++)
        {
            var o = ui3DObjects[i];
            if (o == null) continue;
            o.SetActive(true);
            // restaurar transform local guardado (opcional)
            o.transform.localPosition = originalLocalPos[i];
            o.transform.localRotation = originalLocalRot[i];
            o.transform.localScale = originalLocalScale[i];
        }
    }

    void OnDisable()
    {
        // Intencionalmente vacío: no forzamos apagar los objetos equipados.
    }

    // --- Métodos públicos para equipar / resaltar ---
    // Activa y deja encendido (equipa) el objeto por índice
    public void EquipObject(int index)
    {
        if (!IsValidIndex(index)) return;
        var o = ui3DObjects[index];
        o.SetActive(true);
        // asegurar escala original si por si acaso
        o.transform.localScale = originalLocalScale[index];
    }

    // Resalta (agranda) el objeto por índice; el anterior resaltado vuelve a su escala original.
    // Si index == -1 desresalta todo.
    public void HighlightObject(int index)
    {
        if (index == highlightedIndex) return; // nada que hacer

        // devolver anterior a escala original
        if (IsValidIndex(highlightedIndex) && ui3DObjects[highlightedIndex] != null)
        {
            StartScaleToOriginal(highlightedIndex);
        }

        highlightedIndex = -1;

        // si index válido, animar nuevo highlight
        if (IsValidIndex(index) && ui3DObjects[index] != null)
        {
            // asegurar que el objeto esté activo (si está equipado debería estarlo)
            if (!ui3DObjects[index].activeInHierarchy) ui3DObjects[index].SetActive(true);

            float mul = GetMultiplierForIndex(index);
            Vector3 target = originalLocalScale[index] * mul;
            StartScaleCoroutine(index, ui3DObjects[index].transform.localScale, target, scaleDuration);

            highlightedIndex = index;
        }
    }

    // Desresalta el actual (vuelve a escala original)
    public void UnhighlightCurrent()
    {
        if (IsValidIndex(highlightedIndex))
        {
            StartScaleToOriginal(highlightedIndex);
            highlightedIndex = -1;
        }
    }

    // Desresalta todos (animado)
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

    // --- helpers internos ---
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
            float sp = p * p * (3f - 2f * p);
            t.localScale = Vector3.LerpUnclamped(from, to, sp);
            yield return null;
        }
        t.localScale = to;
        scaleCoroutines[index] = null;
    }

    private float GetMultiplierForIndex(int index)
    {
        if (perObjectMultiplier != null && perObjectMultiplier.Length == ui3DObjects.Length)
            return Mathf.Max(0.01f, perObjectMultiplier[index]);
        return defaultScaleMultiplier;
    }

    private bool IsValidIndex(int idx) => ui3DObjects != null && idx >= 0 && idx < ui3DObjects.Length && ui3DObjects[idx] != null;

#if UNITY_EDITOR
    [ContextMenu("UnhighlightAll")]
    private void Editor_UnhighlightAll() => UnhighlightAll();
#endif
}