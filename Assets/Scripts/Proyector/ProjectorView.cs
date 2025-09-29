using System;
using System.Collections;
using TMPro; // Si no usás TextMeshPro, reemplazá TMP_Text por UnityEngine.UI.Text
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

    Coroutine typeCoroutine;
    Coroutine moveCoroutine;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        if (dialogText == null)
        {
            Debug.LogWarning("ProjectorView: dialogText no asignado.");
        }
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
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

    #region Typewriter
    private IEnumerator TypeTextCoroutine(string fullText, float charDelay, Action onComplete)
    {
        IsTyping = true;
        if (dialogText != null) dialogText.text = "";
        int len = fullText?.Length ?? 0;
        for (int i = 0; i < len; i++)
        {
            if (dialogText != null) dialogText.text += fullText[i];
            yield return new WaitForSecondsRealtime(charDelay);
        }

        IsTyping = false;
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
    }

    public void SetFullTextInstant(string text)
    {
        if (dialogText != null) dialogText.text = text;
        IsTyping = false;
    }
    #endregion
}
