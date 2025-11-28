using UnityEngine;
using System;

// VISTA: El objeto recolectable en el suelo.
[RequireComponent(typeof(Collider))]
public class MapPieceView : MonoBehaviour
{
    // Evento estático para desacoplar. El controlador se suscribirá a esto.
    public static event Action OnPieceCollected;

    [Header("Configuración")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    // Estado interno para saber si el jugador está parado encima
    private bool _isPlayerInRange = false;

    private void Update()
    {
        // Si el jugador está cerca Y presiona la tecla E
        if (_isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            CollectPiece();
        }
    }

    private void CollectPiece()
    {
        // Avisar al sistema principal
        OnPieceCollected?.Invoke();

        // Desactivar el objeto (efecto de "recogido")
        gameObject.SetActive(false);

        // Resetear la variable por seguridad
        _isPlayerInRange = false;
    }

    // Detectar cuando el jugador entra en el área
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            // Opcional: Aquí podrías mostrar un cartelito "Presiona E" en la UI
            // Debug.Log("Presiona E para recoger el mapa"); 
        }
    }

    // Detectar cuando el jugador sale del área
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            // Opcional: Ocultar el cartelito "Presiona E"
        }
    }
}