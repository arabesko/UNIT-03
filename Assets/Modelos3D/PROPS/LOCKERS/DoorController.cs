using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Configuración de Puerta")]
    public float openAngle = 90f;
    public float rotationSpeed = 5f;
    public Vector3 rotationAxis = Vector3.up;
    public bool clockwise = true;

    [Header("Interacción")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRadius = 2f;
    public Vector3 interactionOffset = Vector3.zero;

    [Header("Sonidos")]
    public AudioClip openSound;
    

    // Variables privadas
    private bool isOpen = false;
    private bool isMoving = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    [SerializeField]private AudioSource audioSource;
    private Transform player;
    private bool hasBeenOpened = false; // Nueva variable para controlar si ya se abrió

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Obtener o crear AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configurar AudioSource con valores por defecto
        audioSource.spatialBlend = 1f; // Sonido 3D
        audioSource.playOnAwake = false;

        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        // Verificar distancia con el jugador
        Vector3 interactionPoint = transform.position + interactionOffset;
        float distance = Vector3.Distance(interactionPoint, player.position);

        // Detectar input cuando el jugador está cerca y la puerta no ha sido abierta
        if (distance <= interactionRadius && Input.GetKeyDown(interactionKey) && !hasBeenOpened)
        {
            OpenDoor();
        }

        // Rotación suave
        if (transform.localRotation != targetRotation)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void OpenDoor()
    {
        if (isMoving || hasBeenOpened) return; // Evitar abrir si ya se está moviendo o ya fue abierta

        isOpen = true;
        isMoving = true;
        hasBeenOpened = true; // Marcar que ya fue abierta

        // Calcular dirección de apertura
        float direction = clockwise ? 1f : -1f;
        targetRotation = closedRotation * Quaternion.AngleAxis(openAngle * direction, rotationAxis);

        PlaySound(openSound);
        Invoke("ResetMovementFlag", 0.5f);
    }

    // Eliminamos el método ToggleDoor ya que no lo necesitamos más
    // public void ToggleDoor() { ... }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void ResetMovementFlag() => isMoving = false;

    void OnDrawGizmosSelected()
    {
        // Dibujar área de interacción
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + interactionOffset, interactionRadius);

        // Dibujar eje de rotación
        Gizmos.color = Color.red;
        Vector3 pivotPoint = transform.position;
        Vector3 axisDirection = transform.TransformDirection(rotationAxis) * 0.3f;
        Gizmos.DrawLine(pivotPoint - axisDirection, pivotPoint + axisDirection);
        Gizmos.DrawSphere(pivotPoint, 0.05f);
    }
}