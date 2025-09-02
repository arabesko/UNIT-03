using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelControles;
    public GameObject panelOpciones;

    public void Jugar()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void MostrarControles()
    {
        panelMenu.SetActive(false);
        panelControles.SetActive(true);
    }

    public void Options()
    {
        panelMenu.SetActive(false);
        panelOpciones.SetActive(true);
    }

    public void CloseOptions()
    {
        panelOpciones.SetActive(false);
        panelMenu.SetActive(true);
    }

    public void VolverAlMenu()
    {
        panelControles.SetActive(false);
        panelMenu.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
