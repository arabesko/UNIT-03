using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Configuración de Palanca")]
    public int leverNumber = 1; // 1 o 2, para identificar qué palanca es
    public LeverPuzzleManager puzzleManager;

    [Header("Animación")]
    public Animator leverAnimator;
    public string activateAnimationName = "Activate";

    [Header("UI")]
    public GameObject interactionPrompt; // Texto o UI que muestra "Presiona E"

    private bool playerInRange = false;
    private bool isActivated = false;

    private void Update()
    {
        // Verificar si el jugador está en rango y presiona E
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isActivated)
        {
            ActivateLever();
        }

        // Mostrar/ocultar prompt de interacción
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(playerInRange && !isActivated);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void ActivateLever()
    {
        if (isActivated) return;

        isActivated = true;

        // Reproducir animación
        if (leverAnimator != null)
        {
            leverAnimator.Play(activateAnimationName);
        }

        // Notificar al manager
        if (puzzleManager != null)
        {
            puzzleManager.ActivateLever(leverNumber);
        }

        // Ocultar prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}