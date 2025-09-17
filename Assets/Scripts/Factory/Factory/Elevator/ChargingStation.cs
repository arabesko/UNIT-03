using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChargingStation : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject chargingPromptPanel;
    [SerializeField] private TextMeshProUGUI chargingPromptText;

    [Header("Battery Movement")]
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    private PortableBattery currentBattery;
    private bool isMovingBattery = false;
    private int currentWaypointIndex = 0;
    private bool isMovementCompleted = false;

    // Referencia al script del jugador
    private PlayerMovement _playerScript;

    private void Start()
    {
        // Buscar el script del jugador al inicio
        _playerScript = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {
        if (isMovingBattery && currentBattery != null)
        {
            MoveBatteryAlongWaypoints();
        }
    }

    private void MoveBatteryAlongWaypoints()
    {
        if (currentWaypointIndex >= waypoints.Count || waypoints.Count == 0)
        {
            Debug.LogError("No hay waypoints configurados o índice fuera de rango");
            isMovingBattery = false;
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - currentBattery.transform.position).normalized;

        currentBattery.transform.position += direction * moveSpeed * Time.deltaTime;
        RotateBatteryTowards(target.position);

        float distance = Vector3.Distance(currentBattery.transform.position, target.position);

        if (distance < 0.1f)
        {
            currentWaypointIndex++;
        }

        if (currentWaypointIndex >= waypoints.Count)
        {
            Debug.Log("Movimiento completado");
            isMovingBattery = false;
            isMovementCompleted = true;

            // Iniciar carga después del movimiento
            if (currentBattery != null && !currentBattery.isCharged)
            {
                currentBattery.StartCharging(this);

                if (chargingPromptPanel != null)
                    chargingPromptPanel.SetActive(true);

                if (chargingPromptText != null)
                    chargingPromptText.text = "Cargando bateria...";
            }
        }
    }

    private void RotateBatteryTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - currentBattery.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            currentBattery.transform.rotation = Quaternion.Slerp(
                currentBattery.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entrado: {other.name}, tag: {other.tag}");

        // Detectar cuando la batería entra en el trigger del cargador
        if (other.CompareTag("Battery"))
        {
            Debug.Log("Batería detectada en trigger del cargador");

            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null && !isMovingBattery && !isMovementCompleted)
            {
                // Quitar la batería del control del jugador
                if (_playerScript != null)
                {
                    // Asegurarse de que la batería esté en la lista de colectables del jugador
                    if (_playerScript.colectables.Contains(battery.gameObject))
                    {
                        _playerScript.colectables.Remove(battery.gameObject);
                        _playerScript.NoLevitate();
                        Debug.Log("Batería removida del control del jugador");
                    }
                }

                Debug.Log("Iniciando movimiento de batería");
                // Iniciar movimiento automático
                isMovingBattery = true;
                currentBattery = battery;

                Rigidbody rb = currentBattery.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    Debug.Log("Rigidbody kinematic activado");
                }

                currentWaypointIndex = 0;

                // NO desactivar el script de la batería - en su lugar, usar un flag
                currentBattery.isBeingMoved = true; // Necesitamos agregar este flag al script PortableBattery
                Debug.Log("Batería en movimiento - flag activado");
            }
        }
        // Detectar cuando el jugador se acerca
        else if (other.CompareTag("Player"))
        {
            if (chargingPromptPanel != null)
                chargingPromptPanel.SetActive(true);

            if (chargingPromptText != null)
            {
                if (isMovementCompleted && currentBattery != null && currentBattery.isCharged)
                    chargingPromptText.text = "Batería cargada lista para recoger.";
                else
                    chargingPromptText.text = "Parece una fuente de energia.";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (chargingPromptPanel != null)
                chargingPromptPanel.SetActive(false);
        }
    }

    // Método para reiniciar el estado cuando el jugador recoge la batería cargada
    public void BatteryPickedUp()
    {
        isMovementCompleted = false;

        if (currentBattery != null)
        {
            currentBattery.isBeingMoved = false; // Restablecer el flag
            currentBattery = null;
        }

        if (chargingPromptPanel != null)
            chargingPromptPanel.SetActive(false);
    }

    public void HideChargingText()
    {
        if (chargingPromptPanel != null)
            chargingPromptPanel.SetActive(false);
    }
}