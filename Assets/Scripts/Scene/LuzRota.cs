using System.Collections;
using UnityEngine;

public class LuzRota : MonoBehaviour
{
    public bool _titilo = false;
    public float _timeDelay;

    [Header("Material Titilante")]
    public Material materialTitilante; // Solo asigna el material aquí

    [Header("Modo de Animación")]
    public bool modoAlarma = false; // Si está activo, usa animación lenta tipo alarma

    [Header("Configuración Alarma")]
    public float tiempoEncendido = 1.0f;
    public float tiempoApagado = 1.0f;
    public float velocidadTransicion = 2.0f; // Velocidad de la transición suave

    private Light luz;
    private Color colorOriginal;
    private Color emissionOriginal;
    private bool tieneEmission;
    private float intensidadLuzOriginal;

    void Start()
    {
        luz = GetComponent<Light>();
        intensidadLuzOriginal = luz.intensity;

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
            if (modoAlarma)
            {
                StartCoroutine(AlarmaSuave());
            }
            else
            {
                StartCoroutine(LuzTitilante());
            }
        }
    }

    IEnumerator LuzTitilante()
    {
        _titilo = true;

        // Apagar luz y material
        luz.enabled = false;
        if (materialTitilante != null)
        {
            materialTitilante.color = Color.black;
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", Color.black);
            }
        }

        _timeDelay = Random.Range(0.01f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);

        // Encender luz y restaurar material a su estado original
        luz.enabled = true;
        if (materialTitilante != null)
        {
            materialTitilante.color = colorOriginal;
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", emissionOriginal);
            }
        }

        _timeDelay = Random.Range(0.02f, 0.3f);
        yield return new WaitForSeconds(_timeDelay);

        _titilo = false;
    }

    IEnumerator AlarmaSuave()
    {
        _titilo = true;

        // Transición suave de apagado a encendido
        yield return StartCoroutine(TransicionSuave(false, true, velocidadTransicion));

        // Mantener encendido por tiempo configurado
        yield return new WaitForSeconds(tiempoEncendido);

        // Transición suave de encendido a apagado
        yield return StartCoroutine(TransicionSuave(true, false, velocidadTransicion));

        // Mantener apagado por tiempo configurado
        yield return new WaitForSeconds(tiempoApagado);

        _titilo = false;
    }

    IEnumerator TransicionSuave(bool desdeEncendido, bool haciaEncendido, float duracion)
    {
        float tiempo = 0f;

        Color colorInicio, colorFin;
        Color emissionInicio, emissionFin;
        float intensidadInicio, intensidadFin;

        // Configurar valores iniciales y finales según la dirección de la transición
        if (desdeEncendido && !haciaEncendido)
        {
            // De encendido a apagado
            colorInicio = colorOriginal;
            colorFin = Color.black;
            emissionInicio = tieneEmission ? emissionOriginal : Color.black;
            emissionFin = Color.black;
            intensidadInicio = intensidadLuzOriginal;
            intensidadFin = 0f;
        }
        else
        {
            // De apagado a encendido
            colorInicio = Color.black;
            colorFin = colorOriginal;
            emissionInicio = Color.black;
            emissionFin = tieneEmission ? emissionOriginal : Color.black;
            intensidadInicio = 0f;
            intensidadFin = intensidadLuzOriginal;
        }

        // Asegurar que la luz esté activa durante las transiciones
        luz.enabled = true;

        // Ejecutar la transición suave
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;

            // Interpolar valores
            if (materialTitilante != null)
            {
                materialTitilante.color = Color.Lerp(colorInicio, colorFin, progreso);
                if (tieneEmission)
                {
                    materialTitilante.SetColor("_EmissionColor", Color.Lerp(emissionInicio, emissionFin, progreso));
                }
            }

            luz.intensity = Mathf.Lerp(intensidadInicio, intensidadFin, progreso);

            yield return null;
        }

        // Asegurar valores finales exactos
        if (materialTitilante != null)
        {
            materialTitilante.color = colorFin;
            if (tieneEmission)
            {
                materialTitilante.SetColor("_EmissionColor", emissionFin);
            }
        }

        luz.intensity = intensidadFin;

        // Si está completamente apagado, desactivar la luz
        if (!haciaEncendido)
        {
            luz.enabled = false;
        }
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
        luz.intensity = intensidadLuzOriginal;
        luz.enabled = true;
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
        luz.intensity = intensidadLuzOriginal;
        luz.enabled = true;
    }
}