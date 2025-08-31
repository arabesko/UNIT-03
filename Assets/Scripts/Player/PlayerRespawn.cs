using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;         // Punto donde reaparece el jugador
    public float teleportDelay = 1f;       // Tiempo de espera antes de teletransportar al jugador

    [Header("Global Volume Reference")]
    public Volume globalVolume;            // El Global Volume para el efecto
    public float volumeEffectDuration = 2f; // Tiempo total que el efecto permanece activo
    public float fadeDuration = 1f;         // Tiempo de transición para el fade in/out

    private bool isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isProcessing && other.CompareTag("DeathZone"))
        {
            StartCoroutine(RespawnWithVolumeFade());
        }
    }

    private IEnumerator RespawnWithVolumeFade()
    {
        isProcessing = true;

        // Activar Global Volume gradualmente (Fade In)
        if (globalVolume != null)
        {
            globalVolume.enabled = true;
            yield return StartCoroutine(FadeVolume(0f, 1f));
        }

        // Esperar antes de teletransportar
        yield return new WaitForSeconds(teleportDelay);

        // Teletransportar al jugador
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = respawnPoint.position;
        if (cc != null) cc.enabled = true;

        // Esperar duración del efecto completo (menos lo que ya esperamos antes)
        float remainingEffectTime = Mathf.Max(0f, volumeEffectDuration - teleportDelay);
        yield return new WaitForSeconds(remainingEffectTime);

        // Desactivar Global Volume gradualmente (Fade Out)
        if (globalVolume != null)
        {
            yield return StartCoroutine(FadeVolume(1f, 0f));
            globalVolume.enabled = false;
        }

        isProcessing = false;
    }

    private IEnumerator FadeVolume(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            globalVolume.weight = Mathf.Lerp(from, to, t);
            yield return null;
        }

        globalVolume.weight = to;
    }
}
