using UnityEngine;
using TMPro;
using System.Collections;

public class ElevatorPlayerDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ElevatorPower elevatorPower;
    [SerializeField] private GameObject elevatorPromptPanel;
    [SerializeField] private TextMeshProUGUI elevatorPromptText;
    [SerializeField] private GameObject batteryPromptPanel;
    [SerializeField] private TextMeshProUGUI batteryPromptText;

    [Header("Elevator Settings")]
    [SerializeField] private GameObject elevator;
    [SerializeField] private Transform upPosition;
    [SerializeField] private Transform downPosition;
    [SerializeField] private float speed = 2f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip elevatorMoveClip;

    private bool playerOnPlatform = false;
    private bool isMoving = false;
    private bool isAtBottom = false; // Comienza en la parte superior

    private void Start()
    {
        // Inicializar posición del elevador
        elevator.transform.position = upPosition.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPlatform = true;

            if (elevatorPower.HasPower)
            {
                // Si hay energía, intentar mover el elevador
                TryMoveElevator();
            }
            else
            {
                // Mostrar mensaje de falta de energía
                if (batteryPromptPanel != null)
                    batteryPromptPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPlatform = false;

            if (batteryPromptPanel != null)
                batteryPromptPanel.SetActive(false);
        }
    }

    public void TryMoveElevator()
    {
        if (playerOnPlatform && elevatorPower.HasPower && !isMoving)
        {
            StartCoroutine(MoveElevator());
        }
    }

    IEnumerator MoveElevator()
    {
        isMoving = true;

        if (audioSource != null && elevatorMoveClip != null)
        {
            audioSource.PlayOneShot(elevatorMoveClip);
        }

        Vector3 target = isAtBottom ? upPosition.position : downPosition.position;

        while (Vector3.Distance(elevator.transform.position, target) > 0.05f)
        {
            elevator.transform.position = Vector3.MoveTowards(
                elevator.transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }

        // Actualizar estado de posición
        isAtBottom = !isAtBottom;
        isMoving = false;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // Llamado desde el ElevatorPower cuando se activa la energía
    public void OnPowerActivated()
    {
        // Si el jugador ya está en la plataforma, mover el elevador
        if (playerOnPlatform)
        {
            TryMoveElevator();
        }
    }
}