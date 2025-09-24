using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelControles;
    public GameObject panelOpciones;

    private void Start()
    {
        // Si el panel de opciones estuviera activo al inicio, forzamos la sincronización
        if (panelOpciones != null && panelOpciones.activeSelf)
        {
            StartCoroutine(RefreshAudioBindingsNextFrame());
        }
    }

    public void Jugar()
    {
        // Asegurarse de que el tiempo esté normal antes de cargar la escena
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void MostrarControles()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelControles != null) panelControles.SetActive(true);
    }

    public void Options()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);

        // Esperar 1 frame y luego pedir al MusicManager que (re)asigne los sliders activos
        StartCoroutine(RefreshAudioBindingsNextFrame());
    }

    public void CloseOptions()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelMenu != null) panelMenu.SetActive(true);
    }

    public void VolverAlMenu()
    {
        if (panelControles != null) panelControles.SetActive(false);
        if (panelMenu != null) panelMenu.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ---------- Helper ----------
    private IEnumerator RefreshAudioBindingsNextFrame()
    {
        // Espera un frame para que Unity marque como activos los GameObjects recién activados
        yield return null;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.RefreshUIBindings();
        }
    }
}