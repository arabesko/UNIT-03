using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UImodules3D : MonoBehaviour
{
    [Header("Referencias Visuales")]
    [Tooltip("Los modelos 3D de las tarjetas SD.")]
    public GameObject[] ui3DObjects;

    [Tooltip("Los marcos/bordes que indican selección.")]
    public GameObject[] highlightBorders;

    [Header("Configuración Visual")]
    public float animationDuration = 0.2f;
    public float selectedScale = 1.3f;     // Escala al estar seleccionado
    public float normalScale = 1.0f;       // Escala normal (equipado pero no en uso)

    // Guardamos las escalas originales por si los modelos son distintos
    private Vector3[] initialScales;
    private Coroutine[] activeCoroutines;
    private int currentSelectedIndex = -1;

    void Awake()
    {
        // Inicialización de arrays y guardado de estados iniciales
        if (ui3DObjects != null)
        {
            initialScales = new Vector3[ui3DObjects.Length];
            activeCoroutines = new Coroutine[ui3DObjects.Length];

            for (int i = 0; i < ui3DObjects.Length; i++)
            {
                if (ui3DObjects[i] != null)
                {
                    initialScales[i] = ui3DObjects[i].transform.localScale;
                    // Al inicio, apagamos todo por seguridad
                    ui3DObjects[i].SetActive(false);
                }
            }
        }

        // Apagar todos los marcos al inicio
        if (highlightBorders != null)
        {
            foreach (var border in highlightBorders)
            {
                if (border != null) border.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Se llama SOLO cuando el inventario cambia (agarrar un objeto).
    /// Se encarga de mostrar/ocultar las tarjetas SD disponibles.
    /// </summary>
    public void SyncWithInventory(Inventory inv)
    {
        if (inv == null || ui3DObjects == null) return;

        int itemCount = inv.MyItemsCount();

        for (int i = 0; i < ui3DObjects.Length; i++)
        {
            if (ui3DObjects[i] == null) continue;

            // Verificamos si este slot tiene un item real
            GameObject module = (i < itemCount) ? inv.GetModuleAtIndex(i) : null;
            bool hasItem = module != null;

            // 1. Activar o desactivar el modelo de la tarjeta SD
            if (ui3DObjects[i].activeSelf != hasItem)
            {
                ui3DObjects[i].SetActive(hasItem);
                // Si acabamos de activarlo, asegurar escala normal
                if (hasItem) ui3DObjects[i].transform.localScale = initialScales[i] * normalScale;
            }

            // 2. Si NO tenemos el item, asegurarnos que el borde también esté apagado
            if (!hasItem && highlightBorders != null && i < highlightBorders.Length && highlightBorders[i] != null)
            {
                highlightBorders[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Se llama cuando el jugador presiona 1, 2, 3, 4.
    /// Maneja la escala y el encendido de los marcos.
    /// </summary>
    public void HighlightObject(int index)
    {
        // Si intentamos resaltar algo fuera de rango o nulo, salimos
        if (ui3DObjects == null || index < 0 || index >= ui3DObjects.Length || ui3DObjects[index] == null) return;

        // Si el objeto no está activo (no lo tenemos en inventario), no hacemos nada
        if (!ui3DObjects[index].activeSelf) return;

        // Si ya está seleccionado, no hacemos nada (evita reinicio de animaciones)
        if (currentSelectedIndex == index) return;

        // 1. Deseleccionar el anterior
        if (currentSelectedIndex != -1 && currentSelectedIndex < ui3DObjects.Length)
        {
            AnimateScale(currentSelectedIndex, normalScale);
            ToggleBorder(currentSelectedIndex, false);
        }

        // 2. Seleccionar el nuevo
        currentSelectedIndex = index;
        AnimateScale(currentSelectedIndex, selectedScale);
        ToggleBorder(currentSelectedIndex, true);
    }

    private void ToggleBorder(int index, bool state)
    {
        if (highlightBorders != null && index < highlightBorders.Length && highlightBorders[index] != null)
        {
            highlightBorders[index].SetActive(state);
        }
    }

    private void AnimateScale(int index, float targetMultiplier)
    {
        if (ui3DObjects[index] == null) return;

        if (activeCoroutines[index] != null) StopCoroutine(activeCoroutines[index]);

        Vector3 targetScale = initialScales[index] * targetMultiplier;
        activeCoroutines[index] = StartCoroutine(ScaleCoroutine(ui3DObjects[index].transform, targetScale));
    }

    private IEnumerator ScaleCoroutine(Transform target, Vector3 endScale)
    {
        Vector3 startScale = target.localScale;
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            // Usamos SmoothStep para un movimiento más suave y orgánico
            t = t * t * (3f - 2f * t);

            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        target.localScale = endScale;
    }
}