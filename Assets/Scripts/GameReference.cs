using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameReference : MonoBehaviour
{
    public static GameReference Instance;
    public GameObject player;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // Patrón Singleton mejorado
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
