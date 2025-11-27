using UnityEngine;

// CONTROLADOR: Orquesta la comunicación entre el modelo, la vista y el input.
public class MapSystemController : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] private int totalPieces = 3;
    [SerializeField] private KeyCode inventoryKey = KeyCode.M;

    [Header("Referencias a la Vista")]
    [SerializeField] private MapInspectionUI inspectionUI; // Arrastra aquí el objeto que tiene el script MapInspectionUI

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;   // Sonido al recoger 1 pieza
    [SerializeField] private AudioClip completeSound;  // Sonido al completar el mapa

    // Instancia del Modelo (No es MonoBehaviour, es pura lógica C#)
    private MapModel _model;

    // Estado para saber si tenemos el menú abierto
    private bool _isInspecting = false;

    private void Awake()
    {
        // Inicializamos el modelo con la cantidad de piezas deseadas
        _model = new MapModel(totalPieces);
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento estático de las piezas
        MapPieceView.OnPieceCollected += HandlePieceCollected;
    }

    private void OnDisable()
    {
        // Siempre desuscribirse para evitar errores de memoria
        MapPieceView.OnPieceCollected -= HandlePieceCollected;
    }

    // Se llama automáticamente cuando el Player toca una pieza
    private void HandlePieceCollected()
    {
        _model.AddPiece();

        // Actualizamos la parte visual de los fragmentos 3D
        inspectionUI.UpdateMapFragments(_model.CurrentPieces);

        // Lógica de Sonido y Mensajes
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
        // INPUT: Abrir/Cerrar menú
        // Solo permitimos abrir si ya recogimos al menos una pieza (_model.HasStarted)
        if (_model.HasStarted && Input.GetKeyDown(inventoryKey))
        {
            ToggleInspection();
        }

        // INPUT: Rotación del objeto
        if (_isInspecting)
        {
            HandleRotationInput();
        }
    }

    private void ToggleInspection()
    {
        _isInspecting = !_isInspecting;

        // 1. Avisamos a la UI que se muestre u oculte
        inspectionUI.ToggleInspectionMode(_isInspecting);

        // 2. Manejo del Cursor y Pausa del Juego
        if (_isInspecting)
        {
            // Desbloqueamos el cursor para poder usarlo (si quisieras hacer clic)
            // O simplemente lo dejamos libre para que el movimiento del mouse rote el objeto
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; // O false si prefieres que no se vea la flecha, solo rotar

            // Pausar el tiempo del juego (opcional, estilo Resident Evil)
            Time.timeScale = 0f;
        }
        else
        {
            // Bloqueamos el cursor de nuevo para el FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Reanudar el tiempo
            Time.timeScale = 1f;
        }
    }

    private void HandleRotationInput()
    {
        // Aquí detectamos el movimiento del mouse cuando el menú está abierto.
        // Si prefieres que solo rote cuando haces CLIC y arrastras, descomenta el "if".

        // if (Input.GetMouseButton(0)) 
        // {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Le pasamos los valores a la Vista para que ella se encargue de rotar el Transform
        inspectionUI.RotateObject(mouseX, mouseY);
        // }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}