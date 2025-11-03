using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool _canBlin = true;
    public GameObject player;
    public Transform puntoInicial;

    private string[] validScenes = { "SampleScene", "Level2", "Level3" }; // Agrega tus escenas de juego

    private void Awake()
    {
        // Patrón Singleton mejorado
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Suscribirse al evento
        }
        else
        {
            //Destroy(gameObject);
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name}");

        // Verificar si es una escena donde el jugador debe estar
        bool shouldHavePlayer = System.Array.Exists(validScenes, x => x == scene.name);

        if (shouldHavePlayer)
        {
            InitializePlayer();
        }
        else
        {
            // En escenas como Menu, ocultar o desactivar el jugador
            if (player != null)
            {
                player.SetActive(false);
            }
        }
    }

    private void InitializePlayer()
    {
        StartCoroutine(InitializePlayerCoroutine());
    }

    private IEnumerator InitializePlayerCoroutine()
    {
        // Esperar un frame para asegurar que todo esté cargado
        yield return null;

        // Buscar el jugador si no está asignado
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError("No se encontró el jugador en la escena!");
                yield break;
            }

            DontDestroyOnLoad(player);
        }
        else
        {
            player.SetActive(true);
        }

        // Buscar el punto inicial en la escena actual
        GameObject puntoInicialObj = GameObject.FindGameObjectWithTag("Respawn");
        if (puntoInicialObj != null)
        {
            puntoInicial = puntoInicialObj.transform;
        }

        if (player != null && puntoInicial != null)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.dontDoNothing = false;

                // Desactivar CharacterController momentáneamente para teletransportar
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                    player.transform.position = puntoInicial.position;
                    player.transform.rotation = puntoInicial.rotation;
                    controller.enabled = true;
                }
                else
                {
                    player.transform.position = puntoInicial.position;
                    player.transform.rotation = puntoInicial.rotation;
                }

                // Limpiar lista de coleccionables
                for (int i = 0; i < playerMovement.colectables.Count; i++)
                {
                    if (playerMovement.colectables[i] == null)
                    {
                        playerMovement.colectables.RemoveAt(i);
                        i = 0;
                    }
                }
            }
        }

        // Configurar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDestroy()
    {
        // Desuscribirse del evento para evitar memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}