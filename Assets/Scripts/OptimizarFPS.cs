using UnityEngine;

public class OptimizarFPS : MonoBehaviour
{
    void Awake()
    {
        // Esto desactiva el VSync para que podamos controlar los FPS manualmente
        QualitySettings.vSyncCount = 0;

        // Esto le dice a la GPU: "Solo trabaja hasta llegar a 60, luego descansa"
        Application.targetFrameRate = 75;
    }
}