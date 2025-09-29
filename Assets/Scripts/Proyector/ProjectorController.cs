using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectorView))]
[RequireComponent(typeof(AudioSource))]
public class ProjectorController : MonoBehaviour
{
    [Header("Anchors (World Space)")]
    public Transform defaultAnchor;    // Transform hijo del Canvas (posición por defecto)
    public Transform closeAnchor;      // Transform donde se acerca para hablar
    public float moveDuration = 0.5f;

    [Header("Dialogues")]
    public List<ProjectorDialogue> dialogues = new List<ProjectorDialogue>();

    [Header("Settings")]
    public KeyCode skipKey = KeyCode.E;
    public float globalCharDelay = 0.02f;
    public bool showAtStart = false;
    public bool lockPlayerDuringDialogue = true; // hook para tu sistema de player (implementar si tenés)

    // Dependencias
    ProjectorView view;
    AudioSource audioSource;

    // Estado
    ProjectorDialogue currentDialogue;
    int currentLineIndex;
    Coroutine autoAdvanceCoroutine;
    Coroutine moveCoroutine;

    private void Awake()
    {
        view = GetComponent<ProjectorView>();
        audioSource = GetComponent<AudioSource>();
        if (view == null) Debug.LogError("Se requiere ProjectorView en el mismo GameObject.");
    }

    private void Start()
    {
        // asegurar que el projector esté en default al comenzar
        if (view != null && view.projectorTransform != null && defaultAnchor != null)
        {
            view.projectorTransform.localPosition = defaultAnchor.localPosition;
            view.projectorTransform.localRotation = defaultAnchor.localRotation;
        }

        if (showAtStart && dialogues.Count > 0)
        {
            StartDialogue(0);
        }
        else
        {
            if (view != null) view.HideImmediately();
        }
    }

    private void Update()
    {
        if (currentDialogue != null)
        {
            if (Input.GetKeyDown(skipKey))
            {
                OnSkipPressed();
            }
        }
    }

    #region API de inicio
    public void StartDialogue(int index)
    {
        if (index < 0 || index >= dialogues.Count) return;
        StartDialogue(dialogues[index]);
    }

    public void StartDialogue(string id)
    {
        var d = dialogues.Find(x => x.id == id);
        if (d != null) StartDialogue(d);
        else Debug.LogWarning($"ProjectorController: dialogue id {id} not found.");
    }

    public void StartDialogue(ProjectorDialogue dialogue)
    {
        // Stop general coroutines / estado previo
        StopAllCoroutines();
        currentDialogue = dialogue;
        currentLineIndex = 0;

        // mover a closeAnchor
        if (closeAnchor != null)
        {
            view.StartMove(closeAnchor, moveDuration, this, ref moveCoroutine);
        }

        view.ShowImmediately();
        PlayCurrentLine();
    }
    #endregion

    void PlayCurrentLine()
    {
        if (currentDialogue == null) return;
        if (currentLineIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentLineIndex];

        // reproducir audio de voz (si existe)
        if (line.voiceClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(line.voiceClip);
        }

        // iniciar typewriter
        view.SetFullTextInstant("");
        view.StartTyping(line.text, this, globalCharDelay > 0 ? globalCharDelay : view.defaultCharDelay, () =>
        {
            if (line.autoAdvanceAfter > 0f)
            {
                if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfter(line.autoAdvanceAfter));
            }
        });
    }

    IEnumerator AutoAdvanceAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        AdvanceLine();
    }

    void OnSkipPressed()
    {
        if (currentDialogue == null) return;

        var line = currentDialogue.lines[currentLineIndex];

        if (view.IsTyping)
        {
            // completar instantáneamente la línea
            view.StopTyping(this);
            view.SetFullTextInstant(line.text);

            // si tenía autoAdvance, iniciarlo ahora
            if (line.autoAdvanceAfter > 0f)
            {
                if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfter(line.autoAdvanceAfter));
            }
        }
        else
        {
            // avanzar a la siguiente línea
            AdvanceLine();
        }
    }

    void AdvanceLine()
    {
        if (autoAdvanceCoroutine != null) { StopCoroutine(autoAdvanceCoroutine); autoAdvanceCoroutine = null; }

        currentLineIndex++;
        if (currentLineIndex < currentDialogue.lines.Count)
        {
            PlayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (currentDialogue != null && currentDialogue.closeWhenDone && defaultAnchor != null)
        {
            view.StartMove(defaultAnchor, moveDuration, this, ref moveCoroutine);
        }

        view.HideImmediately();

        // reset state
        currentDialogue = null;
        currentLineIndex = 0;
    }

    // Helper público para triggers externos
    public void TriggerDialogueById(string id)
    {
        StartDialogue(id);
    }
}
