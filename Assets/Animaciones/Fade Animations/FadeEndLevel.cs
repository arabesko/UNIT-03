using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class FadeEndLevel : MonoBehaviour
{
    [Header("Fade Settings")]
    public Volume globalVolume;            // Referencia al Global Volume
    public float fadeDuration = 2f;        // Duración del fade

    [Header("Audio Settings")]
    [SerializeField] private AudioSource ambientAudio;
    [SerializeField] private AudioSource EndTheme;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeOutAndEnd());
        }
    }

    private IEnumerator FadeOutAndEnd()
    {
        // Activar el volumen global si no está activado
        if (globalVolume != null && !globalVolume.enabled)
        {
            globalVolume.enabled = true;
        }

        // Fade in del efecto
        if (globalVolume != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                globalVolume.weight = t;  // Aumentar peso gradualmente
                yield return null;
            }
            globalVolume.weight = 1f;  // Mantener al máximo
        }

        // Manejo de audio
        if (ambientAudio != null && ambientAudio.isPlaying)
            ambientAudio.Pause();

        if (EndTheme != null && !EndTheme.isPlaying)
            EndTheme.Play();
    }
}