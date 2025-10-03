using System.Collections;
using UnityEngine;

public class LuzRota : MonoBehaviour
{
    public bool _titilo = false;
    public float _timeDelay;

    [Header("Material Titilante")]
    public Material materialTitilante; // Asigna el material desde el Inspector
    public Color colorEncendido = Color.white; // Color cuando está "encendido"
    public Color colorApagado = Color.black;   // Color cuando está "apagado"

    private Light luz;
    private Color colorOriginal;

    void Start()
    {
        luz = GetComponent<Light>();

        // Guardar color original del material si existe
        if (materialTitilante != null)
        {
            colorOriginal = materialTitilante.color;
        }
    }

    void Update()
    {
        if (!_titilo)
        {
            StartCoroutine(LuzTitilante());
        }
    }

    IEnumerator LuzTitilante()
    {
        _titilo = true;

        // Apagar luz y material
        luz.enabled = false;
        if (materialTitilante != null)
            materialTitilante.color = colorApagado;

        _timeDelay = Random.Range(0.01f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);

        // Encender luz y material
        luz.enabled = true;
        if (materialTitilante != null)
            materialTitilante.color = colorEncendido;

        _timeDelay = Random.Range(0.02f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);

        _titilo = false;
    }

    // Restaurar color original al desactivar
    void OnDisable()
    {
        if (materialTitilante != null)
            materialTitilante.color = colorOriginal;
    }
}