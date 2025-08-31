using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelControles;

    public void Jugar()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void MostrarControles()
    {
        panelMenu.SetActive(false);
        panelControles.SetActive(true);
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
