using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class RobotEyeFlicker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Luces comunes")]
    public List<Light> lightsToFlicker;

    [Header("Objetos a parpadear")]
    public List<GameObject> objectsToFlicker;

    [Header("Flicker settings")]
    public float flickerDuration = 0.5f;
    public float flickerSpeed = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSfx;

    private Coroutine flickerRoutine;

    void Start()
    {
        // Apagar todo al inicio
        foreach (var l in lightsToFlicker) l.enabled = false;
        foreach (var o in objectsToFlicker) o.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (flickerRoutine != null) StopCoroutine(flickerRoutine);
        flickerRoutine = StartCoroutine(FlickerAndHold());

        if (hoverSfx != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hoverSfx);
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (flickerRoutine != null) StopCoroutine(flickerRoutine);
        foreach (var l in lightsToFlicker) l.enabled = false;
        foreach (var o in objectsToFlicker) o.SetActive(false);
    }

    private IEnumerator FlickerAndHold()
    {
        float elapsed = 0f;
        bool state = false;

        while (elapsed < flickerDuration)
        {
            foreach (var l in lightsToFlicker) l.enabled = state;
            foreach (var o in objectsToFlicker) o.SetActive(state);

            state = !state;
            elapsed += flickerSpeed;
            yield return new WaitForSeconds(flickerSpeed);
        }

        // al final todo queda encendido
        foreach (var l in lightsToFlicker) l.enabled = true;
        foreach (var o in objectsToFlicker) o.SetActive(true);
    }
}
