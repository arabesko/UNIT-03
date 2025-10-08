using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public bool isPaused = false;
    public GameObject optionsPanel;
    public PlayerMovement playerMovement; // Referencia al script del jugador

    void Start()
    {
        // Buscar automáticamente el PlayerMovement si no está asignado
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        // Por si el panel de opciones ya está activo al iniciar (caso raro)
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            StartCoroutine(RefreshAudioBindingsNextFrame());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            else
            {
                if (isPaused) Resume();
                else Pause();
            }
        }
    }

    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        // Forzar mostrar cursor al pausar
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pausar todos los ProjectorControllers en la escena
        PauseAllProjectors(true);

        // Si tus sliders están en el panel de pausa, reconectar UI (esperamos 1 frame)
        StartCoroutine(RefreshAudioBindingsNextFrame());
    }

    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Reanudar todos los ProjectorControllers en la escena
        PauseAllProjectors(false);

        // Dejar que PlayerMovement maneje el estado del cursor
        if (playerMovement != null)
        {
            playerMovement.UpdateCursorState();
        }
    }

    public void Options()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);

        // Esperamos un frame y luego forzamos que MusicManager (re)busque los sliders activos
        StartCoroutine(RefreshAudioBindingsNextFrame());
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);

        // Si el panel de pausa tiene sliders, sincronizarlos también (opcional)
        StartCoroutine(RefreshAudioBindingsNextFrame());
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Buscar y manejar el jugador antes de cambiar escena
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.gameObject.SetActive(false);
            // O si prefieres destruirlo completamente:
            // Destroy(player.gameObject);
        }

        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Salir del juego");
    }

    // ---------- Helpers ----------
    private IEnumerator RefreshAudioBindingsNextFrame()
    {
        // Espera un frame para asegurarse de que GameObjects activados ya estén "visibles" para FindWithTag.
        yield return null;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.RefreshUIBindings();
        }
    }

    // Método para pausar/reanudar todos los ProjectorControllers
    private void PauseAllProjectors(bool pause)
    {
        ProjectorController[] allProjectors = FindObjectsOfType<ProjectorController>();
        foreach (ProjectorController projector in allProjectors)
        {
            projector.SetPaused(pause);
        }
    }
}