using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool _canBlin = true;
    public GameObject player;
    public Transform puntoInicial;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al centro de la pantalla
        Cursor.visible = false; // Oculta el cursor

        player = GameObject.Find("PLAYER GO");
        if (player != null )
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement.dontDoNothing = false;
            playerMovement.transform.position = puntoInicial.position;

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
}
