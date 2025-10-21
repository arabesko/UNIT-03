using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverPuzzleManager : MonoBehaviour
{
    [Header("Puerta")]
    public Transform door;
    public Transform doorOpenPosition;
    public float openSpeed = 1f;

    [Header("Luces")]
    public List<Light> lever1Lights; // Luces para la palanca 1
    public List<Light> lever2Lights; // Luces para la palanca 2

    [Header("Audio - Palancas")]
    public AudioSource leverAudioSource;
    public AudioClip leverActivateSound;

    [Header("Audio - Puerta")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;

    private bool lever1Activated = false;
    private bool lever2Activated = false;
    private bool isDoorOpen = false;

    // Método para activar una palanca
    public void ActivateLever(int leverNumber)
    {
        if (isDoorOpen) return;

        switch (leverNumber)
        {
            case 1:
                if (!lever1Activated)
                {
                    lever1Activated = true;
                    SetLightsColor(lever1Lights, Color.green);

                    if (leverAudioSource != null && leverActivateSound != null)
                    {
                        leverAudioSource.PlayOneShot(leverActivateSound);
                    }

                    CheckPuzzleCompletion();
                }
                break;

            case 2:
                if (!lever2Activated)
                {
                    lever2Activated = true;
                    SetLightsColor(lever2Lights, Color.green);

                    if (leverAudioSource != null && leverActivateSound != null)
                    {
                        leverAudioSource.PlayOneShot(leverActivateSound);
                    }

                    CheckPuzzleCompletion();
                }
                break;
        }
    }

    // Verificar si el puzzle está completo
    private void CheckPuzzleCompletion()
    {
        if (lever1Activated && lever2Activated && !isDoorOpen)
        {
            StartCoroutine(OpenDoor());
            isDoorOpen = true;
        }
    }

    // Cambiar color de las luces
    private void SetLightsColor(List<Light> lights, Color color)
    {
        foreach (Light light in lights)
        {
            if (light != null)
            {
                light.color = color;
                light.enabled = true;
            }
        }
    }

    // Corrutina para abrir la puerta
    private IEnumerator OpenDoor()
    {
        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        float t = 0;
        Vector3 startPos = door.position;
        Vector3 endPos = doorOpenPosition.position;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    // Inicializar luces en rojo al inicio
    private void Start()
    {
        SetLightsColor(lever1Lights, Color.red);
        SetLightsColor(lever2Lights, Color.red);
    }
}