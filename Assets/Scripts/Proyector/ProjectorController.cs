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
    public bool lockPlayerDuringDialogue = true;

    // Dependencias
    ProjectorView view;
    AudioSource audioSource;

    // Estado
    ProjectorDialogue currentDialogue;
    int currentLineIndex;
    Coroutine autoAdvanceCoroutine;
    Coroutine moveCoroutine;

    // Referencia al menú de pausa
    private PauseMenu pauseMenu;
    private bool isPaused = false;

    // Nuevas variables para manejar el estado del auto-avance
    private float remainingAutoAdvanceTime = 0f;
    private bool wasWaitingForAutoAdvance = false;

    private void Awake()
    {
        view = GetComponent<ProjectorView>();
        audioSource = GetComponent<AudioSource>();
        if (view == null) Debug.LogError("Se requiere ProjectorView en el mismo GameObject.");

        // Buscar el PauseMenu en la escena
        pauseMenu = FindObjectOfType<PauseMenu>();
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
        // Verificar si el juego está en pausa
        if (pauseMenu != null)
        {
            isPaused = pauseMenu.isPaused;
        }

        // Si está en pausa, no procesar input de diálogo
        if (isPaused) return;

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
        // Si está en pausa, no iniciar diálogos nuevos
        if (isPaused) return;

        // Resetear estado de auto-avance
        wasWaitingForAutoAdvance = false;
        remainingAutoAdvanceTime = 0f;

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
        if (line.voiceClip != null && audioSource != null && !isPaused)
        {
            audioSource.PlayOneShot(line.voiceClip);
        }

        // iniciar typewriter
        view.SetFullTextInstant("");
        view.StartTyping(line.text, this, globalCharDelay > 0 ? globalCharDelay : view.defaultCharDelay, () =>
        {
            if (line.autoAdvanceAfter > 0f && !isPaused)
            {
                if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfter(line.autoAdvanceAfter));
            }
        });
    }

    IEnumerator AutoAdvanceAfter(float seconds)
    {
        // Usar WaitForSecondsRealtime para que funcione incluso cuando Time.timeScale = 0
        yield return new WaitForSecondsRealtime(seconds);

        // Verificar que no esté en pausa antes de avanzar
        if (!isPaused)
        {
            AdvanceLine();
        }
    }

    void OnSkipPressed()
    {
        if (currentDialogue == null || isPaused) return;

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
        wasWaitingForAutoAdvance = false;
        remainingAutoAdvanceTime = 0f;
    }

    // Helper público para triggers externos
    public void TriggerDialogueById(string id)
    {
        StartDialogue(id);
    }

    // Método público para pausar/despausar desde otros scripts
    public void SetPaused(bool paused)
    {
        bool wasPaused = isPaused;
        isPaused = paused;

        if (isPaused)
        {
            // Pausar audio si está reproduciendo
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }

            // Guardar estado del auto-avance si estaba esperando
            if (autoAdvanceCoroutine != null && currentDialogue != null && currentLineIndex < currentDialogue.lines.Count)
            {
                var currentLine = currentDialogue.lines[currentLineIndex];
                if (currentLine.autoAdvanceAfter > 0f && !view.IsTyping)
                {
                    wasWaitingForAutoAdvance = true;
                    // Detener la corrutina pero guardar que necesitamos reiniciarla
                    StopCoroutine(autoAdvanceCoroutine);
                    autoAdvanceCoroutine = null;
                }
            }
        }
        else
        {
            // Reanudar audio si estaba pausado
            if (audioSource != null)
            {
                audioSource.UnPause();
            }

            // Si acabamos de reanudar y había un diálogo activo
            if (wasPaused && !isPaused && currentDialogue != null)
            {
                // Si estábamos esperando auto-avance antes de pausar, reiniciarlo
                if (wasWaitingForAutoAdvance && currentLineIndex < currentDialogue.lines.Count)
                {
                    var currentLine = currentDialogue.lines[currentLineIndex];
                    if (currentLine.autoAdvanceAfter > 0f && !view.IsTyping)
                    {
                        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
                        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfter(currentLine.autoAdvanceAfter));
                    }
                    wasWaitingForAutoAdvance = false;
                }

                // Si el texto ya estaba completamente mostrado y tenía auto-avance, reiniciarlo
                if (!view.IsTyping && currentLineIndex < currentDialogue.lines.Count)
                {
                    var currentLine = currentDialogue.lines[currentLineIndex];
                    if (currentLine.autoAdvanceAfter > 0f && autoAdvanceCoroutine == null)
                    {
                        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfter(currentLine.autoAdvanceAfter));
                    }
                }
            }
        }
    }

    // Método para verificar si hay un diálogo activo
    public bool IsDialogueActive()
    {
        return currentDialogue != null;
    }
}