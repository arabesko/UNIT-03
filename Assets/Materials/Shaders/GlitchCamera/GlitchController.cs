using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchController : MonoBehaviour
{
    [Header("Parámetros iniciales del glitch")]
    [Tooltip("Cantidad inicial de ruido (noise)")]
    public float noiseAmount = 1f;
    [Tooltip("Fuerza inicial del glitch")]
    public float glitchStrength = 1f;
    [Tooltip("Fuerza inicial de las líneas de escaneo")]
    public float scanLinesStrength = 1f;

    [Header("Duración en segundos para desvanecer el glitch")]
    [Tooltip("Tiempo que tardará en ir de los valores iniciales a 0")]
    public float glitchDuration = 5f;

    [Header("Material del shader (asignar en el Inspector)")]
    public Material mat;

    // Variables internas para almacenar los valores originales
    private float initialNoise;
    private float initialGlitch;
    private float initialScan;

    private float elapsedTime = 0f;

    void Start()
    {
        // Guardamos los valores que pongas en el Inspector como "iniciales"
        initialNoise = noiseAmount;
        initialGlitch = glitchStrength;
        initialScan = scanLinesStrength;

        // Al iniciar la escena, seteamos el material con los valores iniciales inmediatamente
        mat.SetFloat("_NoiseAmount", initialNoise);
        mat.SetFloat("_GlitchStrength", initialGlitch);
        mat.SetFloat("ScanLinesStrength", initialScan);
    }

    void Update()
    {
        if (elapsedTime < glitchDuration)
        {
            // Avanzamos el temporizador
            elapsedTime += Time.deltaTime;

            // Calculamos el porcentaje de progreso (0 al inicio, 1 al final)
            float t = Mathf.Clamp01(elapsedTime / glitchDuration);

            // Interpolamos (lerp) de valor inicial a 0
            float currentNoise = Mathf.Lerp(initialNoise, 0f, t);
            float currentGlitch = Mathf.Lerp(initialGlitch, 0f, t);
            float currentScan = Mathf.Lerp(initialScan, 0f, t);

            // Se los pasamos al shader
            mat.SetFloat("_NoiseAmount", currentNoise);
            mat.SetFloat("_GlitchStrength", currentGlitch);
            mat.SetFloat("ScanLinesStrength", currentScan);
        }
        else
        {
            // Opcional: cuando termine, asegurarse que quede exactamente en cero
            mat.SetFloat("_NoiseAmount", 0f);
            mat.SetFloat("_GlitchStrength", 0f);
            mat.SetFloat("ScanLinesStrength", 0f);
        }
    }
}
