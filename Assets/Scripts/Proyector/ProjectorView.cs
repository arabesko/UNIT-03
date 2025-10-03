using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ProjectorView : MonoBehaviour
{
    [Header("References (World Space UI)")]
    public Transform projectorTransform;   // Transform del objeto UI (world-space RectTransform o un transform común)
    public CanvasGroup canvasGroup;        // opcional para fade in/out
    public TMP_Text dialogText;            // TextMeshPro (si no tenés TMP, sustituir)
    public GameObject speechBubbleRoot;    // root visual que contiene el texto (activar/desactivar)
    public Animator animator;              // opcional: para triggers Speak/Idle/Show/Hide

    [Header("Typewriter")]
    public float defaultCharDelay = 0.02f; // valor por defecto (puede sobreescribirse desde el controller)
    public AnimationCurve moveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Letter float (subtle)")]
    [Tooltip("Amplitud del movimiento (en unidades locales). Muy pequeño -> sutil")]
    public float textAmplitude = 0.6f;
    [Tooltip("Velocidad / frecuencia del movimiento")]
    public float textFrequency = 1.2f;
    [Tooltip("Multiplicador global de velocidad")]
    public float textSpeed = 1f;
    [Tooltip("Si true, solo se anima mientras el ProjectorView esté tipeando")]
    public bool animateOnlyWhileTyping = true;
    [Tooltip("Si true, la animación será aún más sutil (reduce amplitud automáticamente)")]
    public bool extraSubtle = true;

    // internals typewriter
    Coroutine typeCoroutine;
    Coroutine moveCoroutine;
    public bool IsTyping { get; private set; }

    // internals text float
    TMP_Text m_Text;
    bool floatRunning = false;
    float[] charPhases;
    Vector3[][] originalMeshVertices; // guarda mesh sin animaciones
    Coroutine floatCoroutine;

    void Awake()
    {
        if (dialogText == null) Debug.LogWarning("ProjectorView: dialogText no asignado.");
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        m_Text = dialogText;
    }

    #region Show/Hide
    public void ShowImmediately()
    {
        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1;
        if (animator != null) animator.SetTrigger("Show");
    }

    public void HideImmediately()
    {
        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);
        if (animator != null) animator.SetTrigger("Hide");
    }
    #endregion

    #region Movement (World Space: localPosition / localRotation)
    private IEnumerator MoveToCoroutine(Transform target, float duration)
    {
        if (projectorTransform == null || target == null)
            yield break;

        Vector3 startPos = projectorTransform.localPosition;
        Quaternion startRot = projectorTransform.localRotation;
        Vector3 targetPos = target.localPosition;
        Quaternion targetRot = target.localRotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / Mathf.Max(0.0001f, duration));
            float eased = moveEase.Evaluate(normalized);
            projectorTransform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, eased);
            projectorTransform.localRotation = Quaternion.Slerp(startRot, targetRot, eased);
            yield return null;
        }

        projectorTransform.localPosition = targetPos;
        projectorTransform.localRotation = targetRot;
    }

    public void StartMove(Transform target, float duration, MonoBehaviour owner, ref Coroutine outCoroutine)
    {
        if (outCoroutine != null) owner.StopCoroutine(outCoroutine);
        outCoroutine = owner.StartCoroutine(MoveToCoroutine(target, duration));
    }
    #endregion

    #region Typewriter (integrado con float)
    private IEnumerator TypeTextCoroutine(string fullText, float charDelay, Action onComplete)
    {
        IsTyping = true;
        if (m_Text != null) m_Text.text = "";
        int len = fullText?.Length ?? 0;

        // preparar float
        EnsureInitCharPhases(len);
        StartFloatIfNeeded();

        for (int i = 0; i < len; i++)
        {
            if (m_Text != null) m_Text.text += fullText[i];
            // notificar que el texto cambió para recalcular meshes si es necesario
            NotifyTextChangedIfNeeded();
            yield return new WaitForSecondsRealtime(charDelay);
        }

        IsTyping = false;
        // si animar solo mientras escribe, flotado puede detenerse aquí
        if (animateOnlyWhileTyping) StopFloat();

        onComplete?.Invoke();
    }

    public Coroutine StartTyping(string text, MonoBehaviour owner, float charDelay, Action onComplete = null)
    {
        if (typeCoroutine != null) owner.StopCoroutine(typeCoroutine);
        typeCoroutine = owner.StartCoroutine(TypeTextCoroutine(text, charDelay, onComplete));
        return typeCoroutine;
    }

    public void StopTyping(MonoBehaviour owner)
    {
        if (typeCoroutine != null)
        {
            owner.StopCoroutine(typeCoroutine);
            typeCoroutine = null;
            IsTyping = false;
        }
        if (animateOnlyWhileTyping) StopFloat();
    }

    public void SetFullTextInstant(string text)
    {
        if (m_Text != null) m_Text.text = text;
        IsTyping = false;
        NotifyTextChangedIfNeeded();
    }
    #endregion

    #region Text float internals (TMP vertex animation)
    void EnsureInitCharPhases(int required)
    {
        if (charPhases == null || charPhases.Length < required)
        {
            charPhases = new float[Math.Max(required, 4)];
            var rng = new System.Random();
            for (int i = 0; i < charPhases.Length; i++)
            {
                charPhases[i] = (float)(rng.NextDouble() * Math.PI * 2.0);
            }
        }
    }

    void NotifyTextChangedIfNeeded()
    {
        // Force mesh update so we can capture original vertices used as base
        if (m_Text == null) return;
        m_Text.ForceMeshUpdate();
        var info = m_Text.textInfo;
        // Inicializar originalMeshVertices con copia de la mesh actual (sin offset)
        if (info.meshInfo == null || info.meshInfo.Length == 0) return;

        originalMeshVertices = new Vector3[info.meshInfo.Length][];
        for (int i = 0; i < info.meshInfo.Length; i++)
        {
            originalMeshVertices[i] = (Vector3[])info.meshInfo[i].vertices.Clone();
        }
    }

    void StartFloatIfNeeded()
    {
        if (floatRunning) return;

        if (animateOnlyWhileTyping && !IsTyping) return; // no arrancar si solo cuando tipea y no está tipeando

        floatRunning = true;
        if (floatCoroutine != null) StopCoroutine(floatCoroutine);
        floatCoroutine = StartCoroutine(AnimateVertexPositions());
    }

    void StopFloat()
    {
        if (!floatRunning) return;
        floatRunning = false;
        if (floatCoroutine != null)
        {
            StopCoroutine(floatCoroutine);
            floatCoroutine = null;
        }
        // restaurar vertices a su estado original para evitar que queden offsets
        RestoreOriginalVertices();
    }

    IEnumerator AnimateVertexPositions()
    {
        // espera un frame para asegurarnos de que TMP tenga la info
        yield return null;

        // forzar update inicial
        NotifyTextChangedIfNeeded();

        while (floatRunning)
        {
            if (m_Text == null)
            {
                yield return null;
                continue;
            }

            // si solo debe animar mientras se escribe y ya no se escribe -> salir
            if (animateOnlyWhileTyping && !IsTyping)
            {
                yield return null;
                continue;
            }

            m_Text.ForceMeshUpdate();
            var textInfo = m_Text.textInfo;
            int charCount = textInfo.characterCount;
            if (charCount == 0)
            {
                yield return null;
                continue;
            }

            EnsureInitCharPhases(charCount);

            // recomputar original mesh si cambió (ej: distinto número de submeshes)
            if (originalMeshVertices == null || originalMeshVertices.Length != textInfo.meshInfo.Length)
            {
                NotifyTextChangedIfNeeded();
            }

            float t = Time.time * textFrequency * textSpeed;

            for (int i = 0; i < charCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                // phase único por caracter
                float phase = charPhases[i];
                float x = Mathf.Sin(t + phase) * textAmplitude * (extraSubtle ? 0.35f : 0.5f);
                float y = Mathf.Cos(t * 0.9f + phase) * textAmplitude * (extraSubtle ? 0.45f : 0.7f);
                Vector3 offset = new Vector3(x, y, 0f);

                // dst vertices referencian meshInfo.vertices (modificable)
                Vector3[] dstVertices = textInfo.meshInfo[materialIndex].vertices;

                // usar originalMeshVertices como base (si existe), si no, usar current vertices
                Vector3[] srcVertices = originalMeshVertices != null && originalMeshVertices.Length > materialIndex
                    ? originalMeshVertices[materialIndex]
                    : (Vector3[])textInfo.meshInfo[materialIndex].vertices.Clone();

                dstVertices[vertexIndex + 0] = srcVertices[vertexIndex + 0] + offset;
                dstVertices[vertexIndex + 1] = srcVertices[vertexIndex + 1] + offset;
                dstVertices[vertexIndex + 2] = srcVertices[vertexIndex + 2] + offset;
                dstVertices[vertexIndex + 3] = srcVertices[vertexIndex + 3] + offset;
            }

            // actualizar las meshes modificadas
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                m_Text.UpdateGeometry(meshInfo.mesh, i);
            }

            yield return null; // frame a frame
        }
    }

    void RestoreOriginalVertices()
    {
        if (m_Text == null || originalMeshVertices == null) return;
        m_Text.ForceMeshUpdate();
        var info = m_Text.textInfo;
        for (int i = 0; i < info.meshInfo.Length && i < originalMeshVertices.Length; i++)
        {
            var meshInfo = info.meshInfo[i];
            meshInfo.mesh.vertices = originalMeshVertices[i];
            m_Text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
    #endregion

    // Llamar esto desde un script externo si cambias texto manualmente
    public void NotifyTextChangedExternally()
    {
        NotifyTextChangedIfNeeded();
    }

    private void OnDisable()
    {
        // asegurarse de detener coroutines si el objeto se desactiva
        StopFloat();
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }
    }
}
