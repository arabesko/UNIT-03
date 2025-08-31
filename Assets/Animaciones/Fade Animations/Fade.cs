using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Fade : MonoBehaviour
{
    [Header("Global Volume Reference")]
    public Volume globalVolume;

    [Header("Settings")]
    [Tooltip("Time in seconds to fade in/out the volume")]
    public float fadeDuration = 1f;
    [Tooltip("Time in seconds to keep the volume at full strength")]
    public float holdDuration = 2f;

    private bool isProcessing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isProcessing && other.CompareTag("DeathZone"))
        {
            StartCoroutine(TriggerVolumeEffect());
        }
    }

    private IEnumerator TriggerVolumeEffect()
    {
        isProcessing = true;

        if (globalVolume != null)
        {
            globalVolume.weight = 0f;
            globalVolume.enabled = true;

            // Fade in (0 to 1 smoothly)
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                globalVolume.weight = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            globalVolume.weight = 1f;

            // Hold full effect
            yield return new WaitForSeconds(holdDuration);

            // Fade out (1 to 0 smoothly)
            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                globalVolume.weight = Mathf.Clamp01(1f - (timer / fadeDuration));
                yield return null;
            }
            globalVolume.weight = 0f;

            globalVolume.enabled = false;
        }

        isProcessing = false;
    }
}
