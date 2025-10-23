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
    public Transform leverPivot;
    public float rotationAngle = 180f;
    public float rotationSpeed = 3f;
    public bool moveDownward = true;

    [Header("Interacción")]
    public float interactionRadius = 2f;
    public Vector3 interactionOffset = Vector3.zero;

    [Header("Sonidos")]
    public AudioClip activateSound;
    public float soundMaxDistance = 10f;

    // AGREGAR SISTEMA DE LUCES
    [Header("Sistema de Luces")]
    public Light pointLight; // Referencia al Point Light
    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;
    public float lightIntensity = 2f;
    public float lightRange = 3f;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = soundMaxDistance;

        // INICIALIZAR SISTEMA DE LUCES
        InitializeLightSystem();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 interactionPoint = transform.position + interactionOffset;
        float distance = Vector3.Distance(interactionPoint, player.position);
        playerInRange = distance <= interactionRadius;

        // VERIFICACIÓN CORREGIDA - Esta es la parte importante
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isActivated && !isMoving)
        {
            // Verificar si puede activarse
            bool canActivate = true;

            if (requireFuseBoxCompletion)
            {
                if (requiredFuseBox != null)
                {
                    canActivate = requiredFuseBox.IsPuzzleComplete;

                    // Debug para verificar el estado
                    if (!canActivate)
                    {
                        Debug.Log($"Palanca {leverNumber}: La caja de fusibles no está completa. Porcentaje: {requiredFuseBox.TotalPercent}%");
                    }
                }
                else
                {
                    Debug.LogError($"Palanca {leverNumber}: requireFuseBoxCompletion está activado pero requiredFuseBox no está asignado!");
                    canActivate = false;
                }
            }

            if (canActivate)
            {
                ActivateLever();
            }
        }

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

        // CAMBIAR LUZ A VERDE
        UpdateLightColor();

        if (puzzleManager != null)
            puzzleManager.ActivateLever(leverNumber);
    }

    // AGREGAR MÉTODOS PARA EL SISTEMA DE LUCES
    private void InitializeLightSystem()
    {
        // Si no hay light asignada, buscar una en los hijos o crear una
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();

            if (pointLight == null)
            {
                // Crear un nuevo GameObject para la luz
                GameObject lightGO = new GameObject("LeverLight");
                lightGO.transform.SetParent(transform);
                lightGO.transform.localPosition = Vector3.up * 0.5f; // Posición arriba de la palanca

                pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
            }
        }

        // Configurar la luz
        pointLight.color = inactiveColor;
        pointLight.intensity = lightIntensity;
        pointLight.range = lightRange;
        pointLight.enabled = true;
    }

    private void UpdateLightColor()
    {
        if (pointLight != null)
        {
            pointLight.color = isActivated ? activeColor : inactiveColor;
        }
    }

    public void ResetLever()
    {
        isActivated = false;
        isMoving = false;
        leverPivot.localRotation = initialRotation;
        targetRotation = initialRotation;

        // RESTAURAR LUZ A ROJO
        UpdateLightColor();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + interactionOffset, interactionRadius);

        Gizmos.color = Color.red;
        Vector3 pivotPoint = leverPivot != null ? leverPivot.position : transform.position;
        Gizmos.DrawSphere(pivotPoint, 0.05f);

        // AGREGAR GIZMO PARA LA LUZ
        if (pointLight != null)
        {
            Gizmos.color = pointLight.color;
            Gizmos.DrawWireSphere(pointLight.transform.position, 0.2f);
        }
    }
}