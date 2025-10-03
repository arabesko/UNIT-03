using System.Collections;
using UnityEngine;

public class LuzRota : MonoBehaviour
{
    public bool _titilo = false;
    public float _timeDelay;

    [Header("Material Titilante")]
    public Material materialTitilante; // Solo asigna el material aquí

    private Light luz;
    private Color colorOriginal;
    private Color emissionOriginal;
    private bool tieneEmission;

    void Start()
    {
        luz = GetComponent<Light>();

        // Guardar estado original del material
        if (materialTitilante != null)
        {
            colorOriginal = materialTitilante.color;

            // Verificar si el material tiene propiedad de emisión
            if (materialTitilante.HasProperty("_EmissionColor"))
            {
                tieneEmission = true;
                emissionOriginal = materialTitilante.GetColor("_EmissionColor");
            }
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
        {
            materialTitilante.color = Color.black; // Color apagado
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", Color.black); // Emisión apagada
            }
        }

        _timeDelay = Random.Range(0.01f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);

        // Encender luz y restaurar material a su estado original
        luz.enabled = true;
        if (materialTitilante != null)
        {
            materialTitilante.color = colorOriginal; // Color original
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", emissionOriginal); // Emisión original
            }
        }

        _timeDelay = Random.Range(0.02f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);

        _titilo = false;
    }

    // Restaurar estado original al desactivar
    void OnDisable()
    {
        if (materialTitilante != null)
        {
            materialTitilante.color = colorOriginal;
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", emissionOriginal);
            }
        }
    }

    // También restaurar al destruir
    void OnDestroy()
    {
        if (materialTitilante != null)
        {
            materialTitilante.color = colorOriginal;
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", emissionOriginal);
            }
        }
    }
}