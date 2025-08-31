using UnityEngine;
using UnityEngine.Rendering;

public class EagleVision : MonoBehaviour
{
    public enum WaveState { Inactive, Expanding, Fading }

    [Header("Configuración")]
    [SerializeField] private float waveSpeed = 25f;
    [SerializeField] private float maxRadius = 150f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float waveWidth = 10f;

    [Header("Referencias")]
    [SerializeField] private Material visionMaterial; // Material para el efecto de onda expansiva
    [SerializeField] private Volume globalVolume;     // Global Volume para efectos adicionales
    [SerializeField] private float volumeIntensity = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound; // Sonido cuando se activa todo

    [Header("Debug")]
    [SerializeField] private WaveState currentState = WaveState.Inactive;

    private float currentRadius;
    private float fadeTimer;
    private float currentVolumeWeight;

    void Start()
    {
        currentRadius = -1;
        currentVolumeWeight = 0f;

        // Inicializar valores
        UpdateGlobalVolume();
        UpdateMaterial();
    }

    void Update()
    {
        switch (currentState)
        {
            case WaveState.Expanding:
                UpdateExpansion();
                break;
            case WaveState.Fading:
                UpdateFading();
                break;
        }

        // Actualizar el volumen global gradualmente
        if (globalVolume != null)
        {
            float targetWeight = (currentState != WaveState.Inactive) ? volumeIntensity : 0f;
            currentVolumeWeight = Mathf.MoveTowards(currentVolumeWeight, targetWeight, Time.deltaTime * 2f);
            globalVolume.weight = currentVolumeWeight;
        }

        // Actualizar el material siempre (para seguimiento del jugador)
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (visionMaterial != null)
        {
            // Actualizar propiedades del material
            visionMaterial.SetVector("_WaveCenter", transform.position);
            visionMaterial.SetFloat("_WaveRadius", currentRadius);

            // Calcular fade basado en el estado actual
            float fadeValue = 0f;
            switch (currentState)
            {
                case WaveState.Expanding:
                    fadeValue = 1f;
                    break;
                case WaveState.Fading:
                    fadeValue = Mathf.Clamp01(fadeTimer / fadeDuration);
                    break;
                default:
                    fadeValue = 0f;
                    break;
            }

            visionMaterial.SetFloat("_WaveFade", fadeValue);
        }
    }

    void UpdateGlobalVolume()
    {
        if (globalVolume != null)
        {
            globalVolume.weight = currentVolumeWeight;
        }
    }

    public void ActivateVision()
    {
        if (currentState != WaveState.Inactive) return;

        currentState = WaveState.Expanding;
        currentRadius = 0.1f;
        fadeTimer = 0f;
        currentVolumeWeight = 0f;
        PlayActivationSound();
    }

    // Método para reproducir el sonido de activación
    void PlayActivationSound()
    {
        // Verificar que tenemos lo necesario para reproducir sonido
        if (activationSound == null)
        {
            Debug.LogWarning("No hay sonido de activación asignado!");
            return;
        }

        // Intentar reproducir con el AudioSource si está asignado
        if (audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        else
        {
            // Si no hay AudioSource, reproducir en la posición del jugador
            AudioSource.PlayClipAtPoint(activationSound, transform.position);
        }
    }

    void UpdateExpansion()
    {
        currentRadius += waveSpeed * Time.deltaTime;

        if (currentRadius >= maxRadius)
        {
            currentState = WaveState.Fading;
            fadeTimer = fadeDuration;
        }
    }

    void UpdateFading()
    {
        fadeTimer -= Time.deltaTime;
        float fadeValue = Mathf.Clamp01(fadeTimer / fadeDuration);

        // Aplicar curva de desvanecimiento no lineal
        float smoothFade = Mathf.SmoothStep(0f, 1f, fadeValue);

        if (visionMaterial != null)
        {
            visionMaterial.SetFloat("_WaveFade", smoothFade);
        }

        if (globalVolume != null)
        {
            globalVolume.weight = smoothFade * volumeIntensity;
        }

        if (fadeTimer <= 0)
        {
            currentState = WaveState.Inactive;
            currentRadius = -1;
        }
    }

    // Para depuración en el editor
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UpdateGlobalVolume();
            UpdateMaterial();
        }
    }

    void OnDestroy()
    {
        // Limpiar al salir
        if (visionMaterial != null)
        {
            visionMaterial.SetFloat("_WaveRadius", -1);
            visionMaterial.SetFloat("_WaveFade", 0);
        }
    }

    // Métodos para acceder al estado actual (opcional para otros scripts)
    public bool IsActive() => currentState != WaveState.Inactive;
    public float GetCurrentRadius() => currentRadius;
    public float GetCurrentFade() => currentState == WaveState.Fading ?
                                    Mathf.Clamp01(fadeTimer / fadeDuration) : 1f;
}