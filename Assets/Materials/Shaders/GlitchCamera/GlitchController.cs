using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchController : MonoBehaviour
{
    [Header("Parámetros iniciales del glitch")]
    public float noiseAmount = 1f;
    public float glitchStrength = 1f;
    public float scanLinesStrength = 1f;

    [Header("Duración en segundos para desvanecer el glitch")]
    public float glitchDuration = 5f;

    [Header("Material del shader (asignar en el Inspector)")]
    public Material mat;

    private float initialNoise;
    private float initialGlitch;
    private float initialScan;

    private float elapsedTime = 0f;

    void Start()
    {
        initialNoise = noiseAmount;
        initialGlitch = glitchStrength;
        initialScan = scanLinesStrength;

        if (mat != null)
        {
            mat.SetFloat("_NoiseAmount", initialNoise);
            mat.SetFloat("_GlitchStrength", initialGlitch);
            mat.SetFloat("ScanLinesStrength", initialScan);
        }
    }

    void Update()
    {
        if (mat == null) return;

        if (elapsedTime < glitchDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / glitchDuration);

            float currentNoise = Mathf.Lerp(initialNoise, 0f, t);
            float currentGlitch = Mathf.Lerp(initialGlitch, 0f, t);
            float currentScan = Mathf.Lerp(initialScan, 0f, t);

            mat.SetFloat("_NoiseAmount", currentNoise);
            mat.SetFloat("_GlitchStrength", currentGlitch);
            mat.SetFloat("ScanLinesStrength", currentScan);
        }
        else
        {
            mat.SetFloat("_NoiseAmount", 0f);
            mat.SetFloat("_GlitchStrength", 0f);
            mat.SetFloat("ScanLinesStrength", 0f);
        }
    }

    void OnDisable()
    {
        // Esto se ejecuta cuando parás el Play (y también si desactivás el componente).
        if (mat == null) return;

        mat.SetFloat("_NoiseAmount", 0f);
        mat.SetFloat("_GlitchStrength", 0f);
        mat.SetFloat("ScanLinesStrength", 0f);

        // Guardamos el cambio en el asset (solo en Editor)
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(mat);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}
