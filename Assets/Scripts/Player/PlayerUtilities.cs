using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUtilities : MonoBehaviour
{
    [Header("Teletransportación")]
    public Transform puntoF2;
    public Transform puntoF3;
    public Transform puntoF4;

    [Header("Configuración")]
    public KeyCode reiniciarTecla = KeyCode.F1;
    public KeyCode teleportF2 = KeyCode.F2;
    public KeyCode teleportF3 = KeyCode.F3;
    public KeyCode teleportF4 = KeyCode.F4;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Reiniciar juego
        if (Input.GetKeyDown(reiniciarTecla))
        {
            ReiniciarJuego();
        }

        // Teletransportación
        if (Input.GetKeyDown(teleportF2) && puntoF2 != null)
        {
            Teleport(puntoF2);
        }
        if (Input.GetKeyDown(teleportF3) && puntoF3 != null)
        {
            Teleport(puntoF3);
        }
        if (Input.GetKeyDown(teleportF4) && puntoF4 != null)
        {
            Teleport(puntoF4);
        }
    }

    void Teleport(Transform destino)
    {
        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = destino.position;
            characterController.enabled = true;
        }
        else
        {
            transform.position = destino.position;
        }
    }

    void ReiniciarJuego()
    {
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}