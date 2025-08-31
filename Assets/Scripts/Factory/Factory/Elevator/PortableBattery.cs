using UnityEngine;

public class PortableBattery : MonoBehaviour
{
    public bool isCharged = false;
    [SerializeField] private float chargeTime = 5f;
    [SerializeField] private float timer = 0f;
    private bool isCharging = false;

    [Header("UI / SFX")]
    public ChargingStation currentStation; // Referencia a la estación que la está cargando

    [Header("Audio Clips")]
    public AudioSource audioSource;            // Debes asignar aquí un AudioSource en el Inspector
    public AudioClip chargingLoopClip;         // Sonido que suena en bucle mientras carga
    public AudioClip chargedSound;             // Sonido que suena al terminar de cargar

    private void Update()
    {
        if (isCharging)
        {
            timer += Time.deltaTime;
            if (timer >= chargeTime)
            {
                isCharged = true;
                isCharging = false;

                Debug.Log("Batería cargada completamente.");

                // Detenemos el loop de carga (si estaba sonando)
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                // Oculta el texto de carga en la estación y limpia la referencia
                if (currentStation != null)
                {
                    currentStation.HideChargingText();
                    currentStation = null;
                }

                // Reproduce el sonido de batería cargada (solo una vez)
                if (audioSource != null && chargedSound != null)
                {
                    audioSource.PlayOneShot(chargedSound);
                }
            }
        }
    }

    public void StartCharging(ChargingStation station)
    {
        if (!isCharged)
        {
            isCharging = true;
            timer = 0f;
            currentStation = station;

            // Si tenemos AudioSource y clip de loop asignado, lo reproducimos en bucle
            if (audioSource != null && chargingLoopClip != null)
            {
                audioSource.clip = chargingLoopClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    public void StopCharging()
    {
        // Al detener la carga antes de tiempo, paramos el loop y reseteamos timer
        isCharging = false;
        timer = 0f;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        currentStation = null;
    }
}
