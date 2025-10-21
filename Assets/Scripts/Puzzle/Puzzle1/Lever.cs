using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Configuración de Palanca")]
    public int leverNumber = 1;
    public LeverPuzzleManager puzzleManager;

    [Header("Dependencia de Fusibles")]
    public PuzzleFusibles requiredFuseBox; // Caja de fusibles requerida
    public bool requireFuseBoxCompletion = false; // Si requiere que la caja esté completa

    private bool playerInRange = false;
    private bool isActivated = false;

    private void Update()
    {
        // Verificar si se puede activar la palanca
        bool canActivate = !isActivated;

        if (requireFuseBoxCompletion && requiredFuseBox != null)
        {
            canActivate = canActivate && requiredFuseBox.IsPuzzleComplete;
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && canActivate)
        {
            ActivateLever();
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

        if (puzzleManager != null)
        {
            puzzleManager.ActivateLever(leverNumber);
        }
    }
}