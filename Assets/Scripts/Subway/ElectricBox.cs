using UnityEngine;
using UnityEngine;

public class ElectricBox : MonoBehaviour
{
    [Header("Referencias de la Trampa")]
    [SerializeField] private GameObject deathZone;
    [SerializeField] private ParticleSystem electricParticles;
    [SerializeField] private ParticleSystem electricParticles2;
    [SerializeField] private ParticleSystem electricParticles3;
    [SerializeField] private ParticleSystem electricParticles4;

    [Header("Sonido")]
    [SerializeField] private AudioSource electricSound; // Sonido eléctrico

    public void DisableTrap()
    {
        // Desactivar zona de muerte
        if (deathZone != null)
            deathZone.SetActive(false);

        // Detener partículas
        if (electricParticles != null)
            electricParticles.Stop();

        if (electricParticles2 != null)
            electricParticles2.Stop();

        if (electricParticles3 != null)
            electricParticles3.Stop();

        if (electricParticles4 != null)
            electricParticles4.Stop();

        // Detener sonido
        if (electricSound != null && electricSound.isPlaying)
            electricSound.Stop();

        // Desactivar script
        enabled = false;
    }
}