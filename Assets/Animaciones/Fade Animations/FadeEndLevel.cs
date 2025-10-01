using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.SceneManagement;

public class FadeEndLevel : MonoBehaviour
{
    [Header("Fade Settings")]
    public Volume globalVolume;            // Referencia al Global Volume
    public float fadeDuration = 2f;        // Duración del fade

    [Header("Audio Settings")]
    [SerializeField] private AudioSource ambientAudio;
    [SerializeField] private AudioSource EndTheme; // sonido de "elevador que se rompe"

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "Level2";

    private bool transitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (transitioning) return;
        if (!other.CompareTag("Player")) return;

        transitioning = true;

        // Destruir el player PASADOS 3 segundos (no tocamos nada más del player)
        //Destroy(other.gameObject, 3f);

        // Iniciar la secuencia de fade + espera por audio + carga de escena
        StartCoroutine(FadeOutAndEnd());
    }

    private IEnumerator FadeOutAndEnd()
    {
        // 1) Activar el volumen global si está asignado y no está activado
        if (globalVolume != null && !globalVolume.enabled)
            globalVolume.enabled = true;

        // 2) Ejecutar fade in del volume (0 -> 1)
        if (globalVolume != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                globalVolume.weight = t;
                yield return null;
            }
            globalVolume.weight = 1f;
        }
        else
        {
            // si no hay volume asignado, esperamos el tiempo para mantener la sensación de transición
            yield return new WaitForSeconds(fadeDuration);
        }

        // 3) Pausar ambient si corresponde
        if (ambientAudio != null && ambientAudio.isPlaying)
            ambientAudio.Pause();

        // 4) Reproducir EndTheme si está asignado y esperar a que termine
        if (EndTheme != null)
        {
            EndTheme.Play();

            // Si el clip está asignado y tiene length, esperar esa duración (más robusto)
            if (EndTheme.clip != null && EndTheme.clip.length > 0.01f)
            {
                yield return new WaitForSeconds(EndTheme.clip.length);
            }
            else
            {
                // Si no hay clip o su length no es fiable, esperar hasta que termine de sonar
                yield return new WaitWhile(() => EndTheme.isPlaying);
            }
        }

        // 5) Cargar la siguiente escena (asíncrono)
        var ao = SceneManager.LoadSceneAsync(nextSceneName);
        while (!ao.isDone)
            yield return null;
    }
}