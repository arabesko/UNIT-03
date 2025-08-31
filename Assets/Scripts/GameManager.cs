using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool _canBlin = true;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al centro de la pantalla
        Cursor.visible = false; // Oculta el cursor
    }
}
