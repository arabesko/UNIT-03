using UnityEngine;

public class MapSystemController : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] private int totalPieces = 3;
    [SerializeField] private KeyCode inventoryKey = KeyCode.M;

    [Header("Configuración de Reset")]
    [Tooltip("Segundos de inactividad antes de que el mapa vuelva a su posición original.")]
    [SerializeField] private float autoResetDelay = 2.0f;

    [Header("Referencias a la Vista")]
    [SerializeField] private MapInspectionUI inspectionUI;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip completeSound;

    private MapModel _model;
    private bool _isInspecting = false;

    // Variable para saber cuándo fue la última vez que tocaste el mapa
    private float _lastInputTime;

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
        inspectionUI.UpdateMapFragments(_model.CurrentPieces);

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
        if (_model.HasStarted && Input.GetKeyDown(inventoryKey))
        {
            ToggleInspection();
        }

        if (_isInspecting)
        {
            HandleInspectionLogic();
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

            // Reseteamos el timer al abrir
            _lastInputTime = Time.unscaledTime;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    private void HandleInspectionLogic()
    {
        // Si el jugador mantiene presionado el click
        if (Input.GetMouseButton(0))
        {
            // Actualizamos el tiempo de la última interacción
            _lastInputTime = Time.unscaledTime;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            inspectionUI.RotateObject(mouseX, mouseY);
        }
        else
        {
            // Si NO está tocando, verificamos cuánto tiempo pasó
            float timeSinceInput = Time.unscaledTime - _lastInputTime;

            if (timeSinceInput > autoResetDelay)
            {
                // Si pasó el tiempo límite, le decimos a la UI que vuelva al inicio suavemente
                inspectionUI.SmoothResetToDefault();
            }
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