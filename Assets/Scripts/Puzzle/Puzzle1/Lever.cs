using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Configuración de Palanca")]
    public int leverNumber = 1;
    public LeverPuzzleManager puzzleManager;

    [Header("Dependencia de Fusibles")]
    public PuzzleFusibles requiredFuseBox;
    public bool requireFuseBoxCompletion = false;

    [Header("Movimiento de Palanca")]
    public Transform leverPivot; // Pivote principal
    public float rotationAngle = 180f; // Ángulo total (de arriba a abajo)
    public float rotationSpeed = 3f;
    public bool moveDownward = true; // True = hacia abajo

    [Header("Interacción")]
    public float interactionRadius = 2f;
    public Vector3 interactionOffset = Vector3.zero;

    [Header("Sonidos")]
    public AudioClip activateSound;
    public float soundMaxDistance = 10f;

    private bool isActivated = false;
    private bool isMoving = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    public AudioSource audioSource;
    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (leverPivot == null)
            leverPivot = transform;

        initialRotation = leverPivot.localRotation;
        targetRotation = initialRotation;

        // Configurar o crear AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configuración básica del sonido 3D
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Sonido 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = soundMaxDistance;
    }

    void Update()
    {
        // Verificar si el jugador está dentro del rango
        if (player != null)
        {
            Vector3 interactionPoint = transform.position + interactionOffset;
            float distance = Vector3.Distance(interactionPoint, player.position);
            playerInRange = distance <= interactionRadius;
        }

        // Verificar si se puede activar la palanca
        bool canActivate = !isActivated && !isMoving;

        if (requireFuseBoxCompletion && requiredFuseBox != null)
        {
            canActivate = canActivate && requiredFuseBox.IsPuzzleComplete;
        }

        // Input de activación
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && canActivate)
        {
            ActivateLever();
        }

        // Rotación suave del pivote
        if (isMoving)
        {
            leverPivot.localRotation = Quaternion.Slerp(
                leverPivot.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(leverPivot.localRotation, targetRotation) < 0.5f)
                isMoving = false;
        }
    }

    public void ActivateLever()
    {
        if (isMoving || isActivated) return;

        isActivated = true;
        isMoving = true;

        float direction = moveDownward ? 1f : -1f;
        targetRotation = initialRotation * Quaternion.Euler(rotationAngle * direction, 0f, 0f);

        PlaySound(activateSound);

        if (puzzleManager != null)
            puzzleManager.ActivateLever(leverNumber);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void ResetLever()
    {
        isActivated = false;
        isMoving = false;
        leverPivot.localRotation = initialRotation;
        targetRotation = initialRotation;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + interactionOffset, interactionRadius);

        Gizmos.color = Color.red;
        Vector3 pivotPoint = leverPivot != null ? leverPivot.position : transform.position;
        Gizmos.DrawSphere(pivotPoint, 0.05f);
    }
}