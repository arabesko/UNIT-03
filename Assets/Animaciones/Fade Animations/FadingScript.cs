using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FadingScript : MonoBehaviour
{
    [Header("Global Volume Settings")]
    [SerializeField] private Volume globalVolume;  // Referencia al Global Volume
    [SerializeField] private float fadeDuration = 5.0f;
    [SerializeField] private bool fadeInAtStart = true; // Activar fade al inicio

    private void Start()
    {
        // Inicializar el Global Volume: al inicio, pantalla negra (peso=1)
        if (globalVolume != null)
        {
            globalVolume.weight = 1f;
            globalVolume.enabled = true;
        }

        if (fadeInAtStart)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeVolume(1f, 0f, fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeVolume(0f, 1f, fadeDuration));
    }

    private IEnumerator FadeVolume(float start, float end, float duration)
    {
        // Asegurarse que el volumen está activado
        if (globalVolume != null && !globalVolume.enabled)
        {
            globalVolume.enabled = true;
        }

        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            globalVolume.weight = Mathf.Lerp(start, end, t);
            yield return null;
        }
        globalVolume.weight = end;

        // Desactivar el volumen si el efecto final es transparente (peso=0)
        if (end == 0f && globalVolume != null)
        {
            globalVolume.enabled = false;
        }
    }
}