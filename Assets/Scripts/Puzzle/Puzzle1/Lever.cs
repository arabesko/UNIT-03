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

    [Header("Sistema de Luces")]
    public Light pointLight;
    public Color inactiveColor = Color.red;
    public Color readyColor = Color.green;
    public float lightIntensity = 2f;
    public float lightRange = 3f;

    [Header("Sistema de Materiales")]
    public List<Renderer> materialRenderers; // Renderers con los materiales a cambiar
    public Color inactiveEmissionColor = Color.red;
    public Color readyEmissionColor = Color.green;
    public float emissionIntensity = 1f;

    private bool isActivated = false;
    private bool isMoving = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    public AudioSource audioSource;
    private Transform player;
    private bool playerInRange = false;
    private bool wasFuseBoxComplete = false;

    // Para restaurar materiales al salir del play mode
    private List<Color> originalEmissionColors = new List<Color>();

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

        InitializeLightSystem();
        InitializeMaterialSystem();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 interactionPoint = transform.position + interactionOffset;
        float distance = Vector3.Distance(interactionPoint, player.position);
        playerInRange = distance <= interactionRadius;

        // ACTUALIZAR COLOR DE LA LUZ Y MATERIALES BASADO EN LA CAJA DE FUSIBLES
        UpdateVisualsBasedOnFuseBox();

        // VERIFICACIÓN DE ACTIVACIÓN
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isActivated && !isMoving)
        {
            bool canActivate = true;

            if (requireFuseBoxCompletion)
            {
                if (requiredFuseBox != null)
                {
                    canActivate = requiredFuseBox.IsPuzzleComplete;

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

    // NUEVO MÉTODO: Actualizar luz y materiales basado en el estado de la caja de fusibles
    private void UpdateVisualsBasedOnFuseBox()
    {
        bool isFuseBoxComplete = false;

        if (requireFuseBoxCompletion && requiredFuseBox != null)
        {
            isFuseBoxComplete = requiredFuseBox.IsPuzzleComplete;
        }
        else if (!requireFuseBoxCompletion)
        {
            // Si no requiere caja de fusibles, siempre está lista
            isFuseBoxComplete = true;
        }

        // Cambiar color solo si el estado ha cambiado
        if (isFuseBoxComplete != wasFuseBoxComplete)
        {
            // Actualizar luz
            if (pointLight != null)
            {
                pointLight.color = isFuseBoxComplete ? readyColor : inactiveColor;
            }

            // Actualizar materiales
            UpdateMaterialsEmission(isFuseBoxComplete);

            wasFuseBoxComplete = isFuseBoxComplete;

            if (isFuseBoxComplete)
            {
                Debug.Log($"Palanca {leverNumber}: Visuales cambiaron a VERDE - Caja de fusibles completada");
            }
        }
    }

    // NUEVO MÉTODO: Inicializar sistema de materiales
    private void InitializeMaterialSystem()
    {
        originalEmissionColors.Clear();

        foreach (Renderer renderer in materialRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Guardar color de emisión original
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    originalEmissionColors.Add(renderer.material.GetColor("_EmissionColor"));
                }
                else
                {
                    originalEmissionColors.Add(Color.black);
                }

                // Configurar emisión inicial
                SetMaterialEmission(renderer.material, inactiveEmissionColor);
            }
        }
    }

    // NUEVO MÉTODO: Actualizar emisión de materiales
    private void UpdateMaterialsEmission(bool isReady)
    {
        Color targetColor = isReady ? readyEmissionColor : inactiveEmissionColor;

        foreach (Renderer renderer in materialRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                SetMaterialEmission(renderer.material, targetColor);
            }
        }
    }

    // NUEVO MÉTODO: Configurar emisión de material
    private void SetMaterialEmission(Material material, Color color)
    {
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", color * emissionIntensity);

            // Asegurar que la emisión esté activada
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

    // NUEVO MÉTODO: Restaurar colores originales
    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < materialRenderers.Count && i < originalEmissionColors.Count; i++)
        {
            if (materialRenderers[i] != null && materialRenderers[i].material != null)
            {
                if (materialRenderers[i].material.HasProperty("_EmissionColor"))
                {
                    materialRenderers[i].material.SetColor("_EmissionColor", originalEmissionColors[i]);
                }
            }
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

    private void InitializeLightSystem()
    {
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();

            if (pointLight == null)
            {
                GameObject lightGO = new GameObject("LeverLight");
                lightGO.transform.SetParent(transform);
                lightGO.transform.localPosition = Vector3.up * 0.5f;

                pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
            }
        }

        pointLight.color = inactiveColor;
        pointLight.intensity = lightIntensity;
        pointLight.range = lightRange;
        pointLight.enabled = true;
    }

    // CORREGIDO: Método de sonido usando PlayOneShot
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip); // Usar PlayOneShot en lugar de Play
        }
    }

    public void ResetLever()
    {
        isActivated = false;
        isMoving = false;
        leverPivot.localRotation = initialRotation;
        targetRotation = initialRotation;

        // Los visuales se mantienen según el estado de la caja de fusibles
        UpdateVisualsBasedOnFuseBox();
    }

    // NUEVO: Restaurar materiales cuando se destruye el objeto (al salir del play mode)
    private void OnDestroy()
    {
        RestoreOriginalMaterials();
    }

    // NUEVO: Restaurar materiales cuando se desactiva el script
    private void OnDisable()
    {
        // Solo restaurar si la aplicación se está cerrando o en editor
        if (!Application.isPlaying)
        {
            RestoreOriginalMaterials();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + interactionOffset, interactionRadius);

        Gizmos.color = Color.red;
        Vector3 pivotPoint = leverPivot != null ? leverPivot.position : transform.position;
        Gizmos.DrawSphere(pivotPoint, 0.05f);

        if (pointLight != null)
        {
            Gizmos.color = pointLight.color;
            Gizmos.DrawWireSphere(pointLight.transform.position, 0.2f);
        }
    }
}