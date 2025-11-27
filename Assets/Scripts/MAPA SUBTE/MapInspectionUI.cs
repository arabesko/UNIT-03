using UnityEngine;
using TMPro;

public class MapInspectionUI : MonoBehaviour
{
    [Header("Elementos del Canvas World Space")]
    [SerializeField] private GameObject inspectionPanel; 
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI hudCounterText; // texto "1/3" que se ve mientras juegas
    [SerializeField] private GameObject mapCompletedMessage; // "MAPA COMPLETO"

    [Header("El Objeto 3D Interactivo")]
    [SerializeField] private Transform mapPivot; // El padre del objeto 3D que rotaremos
    [SerializeField] private GameObject[] mapFragments; // Las 3 partes del modelo 3D (Hijos del pivot)
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Descripciones")]
    [TextArea][SerializeField] private string incompleteDesc = "Un fragmento de mapa del subte. Parece incompleto.";
    [TextArea][SerializeField] private string completeDesc = "Mapa del subte. Muestra una salida de emergencia y un código de acceso por detras.";

    private void Start()
    {
        // Estado inicial
        inspectionPanel.SetActive(false);
        mapCompletedMessage.SetActive(false);
        UpdateMapFragments(0); // Ocultar todas las partes 3D al inicio
    }

    // Actualiza qué partes del mapa 3D se ven según el progreso
    public void UpdateMapFragments(int currentPieces)
    {
        for (int i = 0; i < mapFragments.Length; i++)
        {
            // Activa el fragmento si el índice es menor a las piezas que tenemos
            mapFragments[i].SetActive(i < currentPieces);
        }

        // Actualizar textos
        descriptionText.text = (currentPieces >= mapFragments.Length) ? completeDesc : incompleteDesc;

        // Actualizar HUD pequeño
        if (hudCounterText != null)
        {
            hudCounterText.text = $"{currentPieces}/{mapFragments.Length}";
            if (currentPieces >= mapFragments.Length) hudCounterText.gameObject.SetActive(false);
        }
    }

    public void ShowMapCompletedMessage()
    {
        mapCompletedMessage.SetActive(true);
        // Ocultarlo después de 3 segundos
        Invoke(nameof(HideMapMessage), 3f);
    }

    private void HideMapMessage() => mapCompletedMessage.SetActive(false);

    // Activa/Desactiva el modo inspección
    public void ToggleInspectionMode(bool isOpen)
    {
        inspectionPanel.SetActive(isOpen);
        mapPivot.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            // Resetear rotación para que siempre aparezca de frente al abrirlo
            mapPivot.localRotation = Quaternion.identity;
        }
    }

    // Lógica visual de rotación
    public void RotateObject(float x, float y)
    {
        // Rotar el pivot basado en el input del mouse
        // Invertimos ejes para que se sienta natural (como mover una bola)
        mapPivot.Rotate(Vector3.up, -x * rotationSpeed, Space.World);
        mapPivot.Rotate(Vector3.right, y * rotationSpeed, Space.World);
    }
}