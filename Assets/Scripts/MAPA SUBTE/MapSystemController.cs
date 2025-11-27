using UnityEngine;

// CONTROLADOR: Orquesta la comunicación entre el modelo, la vista y el input.
public class MapSystemController : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] private int totalPieces = 3;
    [SerializeField] private KeyCode inventoryKey = KeyCode.M;

    [Header("Referencias a la Vista")]
    [SerializeField] private MapInspectionUI inspectionUI;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip completeSound;

    private MapModel _model;
    private bool _isInspecting = false;

    private void Awake()
    {
        _model = new MapModel(totalPieces);
    }

    private void OnEnable()
    {
        MapPieceView.OnPieceCollected += HandlePieceCollected;
    }

    private void OnDisable()
    {
        MapPieceView.OnPieceCollected -= HandlePieceCollected;
    }

    private void HandlePieceCollected()
    {
        _model.AddPiece();

        // Actualizamos la vista (HUD y modelo 3D)
        inspectionUI.UpdateMapFragments(_model.CurrentPieces);

        // Sonidos
        if (_model.IsComplete)
        {
            PlaySound(completeSound);
            inspectionUI.ShowMapCompletedMessage();
        }
        else
        {
            PlaySound(collectSound);
        }
    }

    private void Update()
    {
        // Solo permitimos abrir si ya recogimos al menos una pieza
        if (_model.HasStarted && Input.GetKeyDown(inventoryKey))
        {
            ToggleInspection();
        }

        if (_isInspecting)
        {
            HandleRotationInput();
        }
    }

    private void ToggleInspection()
    {
        _isInspecting = !_isInspecting;
        inspectionUI.ToggleInspectionMode(_isInspecting);

        if (_isInspecting)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    private void HandleRotationInput()
    {
        // AHORA ES OBLIGATORIO MANTENER CLICK IZQUIERDO (Botón 0)
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            inspectionUI.RotateObject(mouseX, mouseY);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}