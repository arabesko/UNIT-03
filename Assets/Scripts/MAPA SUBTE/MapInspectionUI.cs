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
    [SerializeField] private Transform mapPivot;
    [SerializeField] private GameObject[] mapFragments;

    [Header("Velocidades")]
    [SerializeField] private float rotationSpeed = 5f;
    [Tooltip("Qué tan rápido vuelve a su lugar original (más alto = más rápido)")]
    [SerializeField] private float resetSmoothSpeed = 2f;

    [Header("Textos")]
    [TextArea][SerializeField] private string incompleteDesc = "Un fragmento de mapa del subte. Parece incompleto.";
    [TextArea][SerializeField] private string completeDesc = "Mapa completo. Salida y código al dorso.";

    private void Start()
    {
        inspectionPanel.SetActive(false);
        if (mapCompletedMessage) mapCompletedMessage.SetActive(false);
        if (hudCounterText) hudCounterText.gameObject.SetActive(false);

        UpdateFragmentsVisibility(0);
    }

    public void UpdateMapFragments(int currentPieces)
    {
        UpdateFragmentsVisibility(currentPieces);

        if (currentPieces > 0)
        {
            AutoCenterMap();
        }

        descriptionText.text = (currentPieces >= mapFragments.Length) ? completeDesc : incompleteDesc;

        if (hudCounterText != null)
        {
            bool showHud = currentPieces > 0 && currentPieces < mapFragments.Length;
            hudCounterText.gameObject.SetActive(showHud);
            hudCounterText.text = $"COMPLETAR MAPA {currentPieces}/{mapFragments.Length}";
        }
    }

    private void UpdateFragmentsVisibility(int currentPieces)
    {
        for (int i = 0; i < mapFragments.Length; i++)
        {
            mapFragments[i].SetActive(i < currentPieces);
        }
    }

    private void AutoCenterMap()
    {
        Quaternion originalRotation = mapPivot.rotation;
        mapPivot.rotation = Quaternion.identity;

        Bounds combinedBounds = new Bounds(mapPivot.position, Vector3.zero);
        bool hasBounds = false;

        foreach (var fragment in mapFragments)
        {
            if (fragment.activeSelf)
            {
                Renderer r = fragment.GetComponent<Renderer>();
                if (r != null)
                {
                    if (!hasBounds) { combinedBounds = r.bounds; hasBounds = true; }
                    else { combinedBounds.Encapsulate(r.bounds); }
                }
            }
        }

        if (hasBounds)
        {
            Vector3 currentCenter = combinedBounds.center;
            Vector3 correctionOffset = mapPivot.position - currentCenter;
            foreach (var fragment in mapFragments)
            {
                fragment.transform.position += correctionOffset;
            }
        }
        mapPivot.rotation = originalRotation;
    }

    public void ToggleInspectionMode(bool isOpen)
    {
        inspectionPanel.SetActive(isOpen);
        mapPivot.gameObject.SetActive(isOpen);

        if (isOpen)
        {
            mapPivot.localRotation = Quaternion.identity;
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

    // --- ROTACIÓN MANUAL ---
    public void RotateObject(float x, float y)
    {
        mapPivot.Rotate(Vector3.up, -x * rotationSpeed, Space.Self);
        mapPivot.Rotate(Vector3.right, y * rotationSpeed, Space.Self);
    }

    // --- RESET AUTOMÁTICO SUAVE ---
    public void SmoothResetToDefault()
    {
        // Interpolamos suavemente (Slerp) desde la rotación actual hacia Identity (0,0,0)
        // Usamos unscaledDeltaTime porque el juego está en pausa (TimeScale = 0)
        mapPivot.localRotation = Quaternion.Slerp(
            mapPivot.localRotation,
            Quaternion.identity,
            Time.unscaledDeltaTime * resetSmoothSpeed
        );
    }
}