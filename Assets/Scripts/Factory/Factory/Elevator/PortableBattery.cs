using UnityEngine;

public class PortableBattery : MonoBehaviour
{
    public bool isCharged = false;
    [SerializeField] private float chargeTime = 5f;
    [SerializeField] private float timer = 0f;
    private bool isCharging = false;
    public bool isBeingMoved = false;

    [Header("UI / SFX")]
    public ChargingStation currentStation;

    [Header("Audio Clips")]
    public AudioSource audioSource;
    public AudioClip chargingLoopClip;
    public AudioClip chargedSound;

    private void Update()
    {
        if (isCharging && !isCharged)
        {
            timer += Time.deltaTime;
            if (timer >= chargeTime)
            {
                isCharged = true;
                isCharging = false;

                Debug.Log("Batería cargada completamente.");

                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                if (currentStation != null)
                {
                    currentStation.BatteryReadyForPickup(); // Notificar que está lista para recoger
                    currentStation.HideChargingText();
                    currentStation = null;
                }

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
        isCharging = false;
        timer = 0f;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        currentStation = null;

        // Reactivar la física si se detiene la carga
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}