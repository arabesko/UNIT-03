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
    public AudioSource audioSource; // Ahora se asigna manualmente
    public AudioClip activateSound;

    [Header("Sistema de Luces")]
    public Light pointLight;
    public Color inactiveColor = Color.red;
    public Color readyColor = Color.green;
    public float lightIntensity = 2f;
    public float lightRange = 3f;

    [Header("Sistema de Materiales")]
    public List<Renderer> materialRenderers;
    public Color inactiveEmissionColor = Color.red;
    public Color readyEmissionColor = Color.green;
    public float emissionIntensity = 1f;

    private bool isActivated = false;
    private bool isMoving = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private Transform player;
    private bool playerInRange = false;
    private bool wasFuseBoxComplete = false;
    private List<Color> originalEmissionColors = new List<Color>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (leverPivot == null)
            leverPivot = transform;

        initialRotation = leverPivot.localRotation;
        targetRotation = initialRotation;

        // SOLUCIÓN DEL SONIDO: No crear AudioSource automáticamente, usar el asignado manualmente
        if (audioSource == null)
        {
            Debug.LogWarning($"Palanca {leverNumber}: No se ha asignado un AudioSource. El sonido no funcionará.");
        }
        else
        {
            // Configuración mínima para el AudioSource
            audioSource.playOnAwake = false;
        }

        InitializeLightSystem();
        InitializeMaterialSystem();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 interactionPoint = transform.position + interactionOffset;
        float distance = Vector3.Distance(interactionPoint, player.position);
        playerInRange = distance <= interactionRadius;

        UpdateVisualsBasedOnFuseBox();

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

    private void UpdateVisualsBasedOnFuseBox()
    {
        bool isFuseBoxComplete = false;

        if (requireFuseBoxCompletion && requiredFuseBox != null)
        {
            isFuseBoxComplete = requiredFuseBox.IsPuzzleComplete;
        }
        else if (!requireFuseBoxCompletion)
        {
            isFuseBoxComplete = true;
        }

        if (isFuseBoxComplete != wasFuseBoxComplete)
        {
            if (pointLight != null)
            {
                pointLight.color = isFuseBoxComplete ? readyColor : inactiveColor;
            }

            UpdateMaterialsEmission(isFuseBoxComplete);
            wasFuseBoxComplete = isFuseBoxComplete;

            if (isFuseBoxComplete)
            {
                Debug.Log($"Palanca {leverNumber}: Visuales cambiaron a VERDE - Caja de fusibles completada");
            }
        }
    }

    private void InitializeMaterialSystem()
    {
        originalEmissionColors.Clear();

        foreach (Renderer renderer in materialRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    originalEmissionColors.Add(renderer.material.GetColor("_EmissionColor"));
                }
                else
                {
                    originalEmissionColors.Add(Color.black);
                }

                SetMaterialEmission(renderer.material, inactiveEmissionColor);
            }
        }
    }

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

    private void SetMaterialEmission(Material material, Color color)
    {
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", color * emissionIntensity);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

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

    // MÉTODO DE SONIDO SIMPLIFICADO
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // SOLUCIÓN: Usar PlayOneShot y asegurar que el AudioSource esté configurado correctamente
            audioSource.PlayOneShot(clip);
            Debug.Log($"Palanca {leverNumber}: Sonido reproducido");
        }
        else
        {
            if (clip == null)
                Debug.LogWarning($"Palanca {leverNumber}: No hay AudioClip asignado");
            if (audioSource == null)
                Debug.LogWarning($"Palanca {leverNumber}: No hay AudioSource asignado");
        }
    }

    public void ResetLever()
    {
        isActivated = false;
        isMoving = false;
        leverPivot.localRotation = initialRotation;
        targetRotation = initialRotation;
        UpdateVisualsBasedOnFuseBox();
    }

    private void OnDestroy()
    {
        RestoreOriginalMaterials();
    }

    private void OnDisable()
    {
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