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
    public AudioClip closeSound;
    [Range(0, 10)] public float volume = 1f;

    // Variables privadas
    private bool isOpen = false;
    private bool isMoving = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        // Verificar distancia con el jugador
        Vector3 interactionPoint = transform.position + interactionOffset;
        float distance = Vector3.Distance(interactionPoint, player.position);

        // Detectar input cuando el jugador está cerca
        if (distance <= interactionRadius && Input.GetKeyDown(interactionKey))
        {
            ToggleDoor();
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

    public void ToggleDoor()
    {
        if (isMoving) return;

        isOpen = !isOpen;
        isMoving = true;

        // Calcular dirección de apertura
        float direction = clockwise ? 1f : -1f;

        if (isOpen)
        {
            targetRotation = closedRotation * Quaternion.AngleAxis(openAngle * direction, rotationAxis);
            PlaySound(openSound);
        }
        else
        {
            targetRotation = closedRotation;
            PlaySound(closeSound);
        }

        Invoke("ResetMovementFlag", 0.5f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
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