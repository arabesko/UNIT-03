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

    [Header("Movimiento de Palanca")]
    public Transform leverPivot; // Pivote común para todas las partes
    public List<Transform> leverParts; // Las 3 partes de la palanca
    public float rotationAngle = 45f; // Ángulo de rotación vertical
    public float rotationSpeed = 2f;
    public bool moveDownward = true; // True: hacia abajo, False: hacia arriba

    [Header("Interacción")]
    public float interactionRadius = 2f;
    public Vector3 interactionOffset = Vector3.zero;

    [Header("Sonidos")]
    public AudioClip activateSound;

    // Variables privadas
    private bool isActivated = false;
    private bool isMoving = false;
    private List<Quaternion> initialRotations = new List<Quaternion>();
    private List<Quaternion> targetRotations = new List<Quaternion>();
    [SerializeField] private AudioSource audioSource;
    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Obtener o crear AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configurar AudioSource con valores por defecto
        audioSource.spatialBlend = 1f; // Sonido 3D
        audioSource.playOnAwake = false;

        // Guardar rotaciones iniciales de todas las partes
        foreach (Transform part in leverParts)
        {
            if (part != null)
            {
                initialRotations.Add(part.localRotation);
                targetRotations.Add(part.localRotation);
            }
        }

        // Si no se asignó un pivote, usar el transform actual
        if (leverPivot == null)
        {
            leverPivot = transform;
        }
    }

    void Update()
    {
        // Verificar distancia con el jugador
        Vector3 interactionPoint = transform.position + interactionOffset;
        float distance = Vector3.Distance(interactionPoint, player.position);

        // Actualizar estado de rango del jugador
        playerInRange = distance <= interactionRadius;

        // Detectar input cuando el jugador está cerca y la palanca no ha sido activada
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isActivated && !isMoving)
        {
            // Verificar dependencia de fusibles si es necesario
            bool canActivate = true;
            if (requireFuseBoxCompletion && requiredFuseBox != null)
            {
                canActivate = requiredFuseBox.IsPuzzleComplete;
            }

            if (canActivate)
            {
                ActivateLever();
            }
        }

        // Rotación suave de todas las partes
        if (isMoving)
        {
            for (int i = 0; i < leverParts.Count; i++)
            {
                if (leverParts[i] != null)
                {
                    leverParts[i].localRotation = Quaternion.Slerp(
                        leverParts[i].localRotation,
                        targetRotations[i],
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            // Verificar si todas las partes han alcanzado su rotación objetivo
            bool allRotated = true;
            for (int i = 0; i < leverParts.Count; i++)
            {
                if (leverParts[i] != null &&
                    Quaternion.Angle(leverParts[i].localRotation, targetRotations[i]) > 0.1f)
                {
                    allRotated = false;
                    break;
                }
            }

            if (allRotated)
            {
                isMoving = false;
            }
        }
    }

    public void ActivateLever()
    {
        if (isMoving || isActivated) return;

        isActivated = true;
        isMoving = true;

        // Calcular rotación objetivo para cada parte
        float direction = moveDownward ? -1f : 1f;

        for (int i = 0; i < leverParts.Count; i++)
        {
            if (leverParts[i] != null)
            {
                // Rotar alrededor del eje X (vertical)
                targetRotations[i] = initialRotations[i] * Quaternion.Euler(rotationAngle * direction, 0, 0);
            }
        }

        // Reproducir sonido
        PlaySound(activateSound);

        // Notificar al manager
        if (puzzleManager != null)
        {
            puzzleManager.ActivateLever(leverNumber);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Método para resetear la palanca (opcional)
    public void ResetLever()
    {
        isActivated = false;
        isMoving = false;

        for (int i = 0; i < leverParts.Count; i++)
        {
            if (leverParts[i] != null)
            {
                leverParts[i].localRotation = initialRotations[i];
                targetRotations[i] = initialRotations[i];
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Dibujar área de interacción
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + interactionOffset, interactionRadius);

        // Dibujar pivote de rotación
        Gizmos.color = Color.red;
        Vector3 pivotPoint = leverPivot != null ? leverPivot.position : transform.position;
        Gizmos.DrawSphere(pivotPoint, 0.05f);

        // Dibujar dirección de rotación
        Gizmos.color = Color.blue;
        Vector3 rotationDirection = (moveDownward ? Vector3.down : Vector3.up) * 0.3f;
        Gizmos.DrawLine(pivotPoint, pivotPoint + rotationDirection);
    }

    // Métodos para mantener compatibilidad con el sistema anterior
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
}