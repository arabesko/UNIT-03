using UnityEngine;
using TMPro;

public class MapInspectionUI : MonoBehaviour
{
    [Header("Elementos UI")]
    [SerializeField] private GameObject inspectionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI hudCounterText;
    [SerializeField] private GameObject mapCompletedMessage;

    [Header("Configuración 3D")]
    [Tooltip("El objeto padre que rota (MapPivot).")]
    [SerializeField] private Transform mapPivot;

    [Tooltip("Arrastra aquí tus 3 objetos del mapa (Parte1, Parte2, Parte3).")]
    [SerializeField] private GameObject[] mapFragments;

    [SerializeField] private float rotationSpeed = 5f;

    [Header("Textos")]
    [TextArea][SerializeField] private string incompleteDesc = "Un fragmento de mapa del subte. Parece incompleto.";
    [TextArea][SerializeField] private string completeDesc = "Mapa completo. Salida y código al dorso.";

    private void Start()
    {
        // Inicialización
        inspectionPanel.SetActive(false);
        if (mapCompletedMessage) mapCompletedMessage.SetActive(false);
        if (hudCounterText) hudCounterText.gameObject.SetActive(false);

        UpdateFragmentsVisibility(0);
    }

    public void UpdateMapFragments(int currentPieces)
    {
        // 1. Activar/Desactivar piezas
        UpdateFragmentsVisibility(currentPieces);

        // 2. FORZAR CENTRADO AUTOMÁTICO (La Solución)
        if (currentPieces > 0)
        {
            AutoCenterMap();
        }

        // 3. Actualizar Textos
        descriptionText.text = (currentPieces >= mapFragments.Length) ? completeDesc : incompleteDesc;

        if (hudCounterText != null)
        {
            bool showHud = currentPieces > 0 && currentPieces < mapFragments.Length;
            hudCounterText.gameObject.SetActive(showHud);
            hudCounterText.text = $"{currentPieces}/{mapFragments.Length}";
        }
    }

    private void UpdateFragmentsVisibility(int currentPieces)
    {
        for (int i = 0; i < mapFragments.Length; i++)
        {
            mapFragments[i].SetActive(i < currentPieces);
        }
    }

    // --- MAGIA MATEMÁTICA AQUÍ ---
    private void AutoCenterMap()
    {
        // 1. Reseteamos la rotación del pivote temporalmente para que los cálculos sean rectos (sin ángulo).
        Quaternion originalRotation = mapPivot.rotation;
        mapPivot.rotation = Quaternion.identity;

        // 2. Calculamos los límites (Bounds) de todo lo que sea visible.
        Bounds combinedBounds = new Bounds(mapPivot.position, Vector3.zero);
        bool hasBounds = false;

        foreach (var fragment in mapFragments)
        {
            if (fragment.activeSelf)
            {
                Renderer r = fragment.GetComponent<Renderer>();
                if (r != null)
                {
                    if (!hasBounds)
                    {
                        combinedBounds = r.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(r.bounds);
                    }
                }
            }
        }

        // 3. Si encontramos geometría visible, la movemos.
        if (hasBounds)
        {
            // El centro actual de la geometría en el mundo
            Vector3 currentCenter = combinedBounds.center;

            // La diferencia entre donde está el pivote y donde está el centro geométrico
            Vector3 correctionOffset = mapPivot.position - currentCenter;

            // Movemos CADA PIEZA individualmente por esa diferencia.
            // Esto alinea el centro visual con el pivote físico.
            foreach (var fragment in mapFragments)
            {
                fragment.transform.position += correctionOffset;
            }
        }

        // 4. Restauramos la rotación que tenía (si la hubiera, aunque al abrir suele ser 0)
        mapPivot.rotation = originalRotation;
    }

    public void ToggleInspectionMode(bool isOpen)
    {
        inspectionPanel.SetActive(isOpen);
        mapPivot.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            // Al abrir, reseteamos la rotación para que se vea de frente siempre
            mapPivot.localRotation = Quaternion.identity;

            // Recalculamos el centro por si acaso
            AutoCenterMap();
        }
    }

    public void ShowMapCompletedMessage()
    {
        if (mapCompletedMessage)
        {
            mapCompletedMessage.SetActive(true);
            Invoke(nameof(HideMapMessage), 3f);
        }
    }

    private void HideMapMessage() => mapCompletedMessage.SetActive(false);

    public void RotateObject(float x, float y)
    {
        // Rotación fijada al objeto (Space.World relativo a cámara suele ser mejor, 
        // pero Space.Self es más estable si la cámara se mueve raro).

        // Eje vertical (Y)
        mapPivot.Rotate(Vector3.up, -x * rotationSpeed, Space.Self);

        // Eje horizontal (X)
        mapPivot.Rotate(Vector3.right, y * rotationSpeed, Space.Self);
    }
}