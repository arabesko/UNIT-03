using UnityEngine;

public class ElectricBox : MonoBehaviour
{
    [Header("Referencias de la Trampa")]
    [SerializeField] private GameObject deathZone; // Referencia al trigger de muerte
    [SerializeField] private ParticleSystem electricParticles; // Referencia al sistema de partículas
    //[SerializeField] private Renderer boxRenderer; // Opcional: para cambiar el material

    //[Header("Configuración")]
    //[SerializeField] private Material damagedMaterial; // Opcional: material cuando se destruye

    public void DisableTrap()
    {
        // Desactivar zona de muerte
        if (deathZone != null)
            deathZone.SetActive(false);

        // Desactivar partículas
        if (electricParticles != null)
            electricParticles.Stop();

        // Opcional: cambiar apariencia de la caja
        /*if (boxRenderer != null && damagedMaterial != null)
            boxRenderer.material = damagedMaterial;*/

        // Desactivar este script para evitar interacciones futuras
        enabled = false;
    }
}