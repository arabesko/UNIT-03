using UnityEngine;
using TMPro;

// VISTA: Maneja TODO lo visual del menú de inspección (Canvas World Space + Objeto 3D).
public class MapInspectionUI : MonoBehaviour
{
    [Header("Elementos del Canvas World Space")]
    [SerializeField] private GameObject inspectionPanel; // El panel negro de fondo + Descripción
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI hudCounterText; // El texto "1/3" del HUD normal
    [SerializeField] private GameObject mapCompletedMessage;

    [Header("El Objeto 3D Interactivo")]
    [SerializeField] private Transform mapPivot;
    [SerializeField] private GameObject[] mapFragments;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Descripciones")]
    [TextArea][SerializeField] private string incompleteDesc = "Un fragmento de mapa del subte. Parece incompleto.";
    [TextArea][SerializeField] private string completeDesc = "Mapa del subte. Muestra una salida de emergencia y un código de acceso al dorso.";

    private void Start()
    {
        inspectionPanel.SetActive(false);
        mapCompletedMessage.SetActive(false);

        // ESTADO INICIAL DEL HUD: OCULTO
        // Como empezamos con 0 piezas, lo apagamos para que no se vea "0/3"
        if (hudCounterText != null) hudCounterText.gameObject.SetActive(false);

        // Ocultar fragmentos 3D
        UpdateFragmentsVisibility(0);
    }

    // Método central para actualizar todo según el progreso
    public void UpdateMapFragments(int currentPieces)
    {
        // 1. Actualizar qué partes del mapa 3D se ven
        UpdateFragmentsVisibility(currentPieces);

        // 2. Actualizar Texto de Descripción (para cuando aprietes M)
        descriptionText.text = (currentPieces >= mapFragments.Length) ? completeDesc : incompleteDesc;

        // 3. ACTUALIZACIÓN DEL HUD INTELIGENTE
        if (hudCounterText != null)
        {
            // Solo mostramos el HUD si tenemos piezas (> 0) Y no hemos terminado
            // Si quieres que el HUD desaparezca al terminar, usa la linea de abajo tal cual.
            // Si quieres que se quede fijo en 3/3, quita la parte de "&& currentPieces < mapFragments.Length"
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

    public void ShowMapCompletedMessage()
    {
        mapCompletedMessage.SetActive(true);
        Invoke(nameof(HideMapMessage), 3f);
    }

    private void HideMapMessage() => mapCompletedMessage.SetActive(false);

    public void ToggleInspectionMode(bool isOpen)
    {
        // Esto activa el panel negro Y la descripción que está dentro de él
        inspectionPanel.SetActive(isOpen);
        mapPivot.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            mapPivot.localRotation = Quaternion.identity;
        }
    }

    public void RotateObject(float x, float y)
    {
        mapPivot.Rotate(Vector3.up, -x * rotationSpeed, Space.World);
        mapPivot.Rotate(Vector3.right, y * rotationSpeed, Space.World);
    }
}