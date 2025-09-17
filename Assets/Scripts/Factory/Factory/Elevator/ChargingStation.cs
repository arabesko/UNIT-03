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
            Debug.Log("Movimiento completado, iniciando carga");
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
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - currentBattery.transform.position).normalized;

        // Movimiento suave usando Lerp
        currentBattery.transform.position = Vector3.Lerp(
            currentBattery.transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        RotateBatteryTowards(target.position);

        // Verificar si hemos llegado al waypoint actual
        if (Vector3.Distance(currentBattery.transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex++;
            Debug.Log("Pasando al siguiente waypoint: " + currentWaypointIndex);
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
        if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null && !battery.isCharged && !isMovingBattery && !isMovementCompleted)
            {
                Debug.Log("Batería detectada, iniciando movimiento");

                // Quitar la batería del control del jugador
                if (_playerScript != null)
                {
                    if (_playerScript.colectables.Contains(battery.gameObject))
                    {
                        _playerScript.colectables.Remove(battery.gameObject);
                        _playerScript.NoLevitate();
                        Debug.Log("Batería removida del control del jugador");
                    }
                }

                // Iniciar movimiento automático
                isMovingBattery = true;
                currentBattery = battery;
                currentWaypointIndex = 0;

                // Hacer la batería kinemática para evitar interferencias físicas
                Rigidbody rb = currentBattery.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                // Marcar que la batería está siendo movida
                currentBattery.isBeingMoved = true;
            }
            // Permitir recoger la batería cargada
            else if (battery != null && battery.isCharged)
            {
                // Agregar la batería a la lista de colectables del jugador
                if (_playerScript != null && !_playerScript.colectables.Contains(battery.gameObject))
                {
                    _playerScript.colectables.Add(battery.gameObject);
                    Debug.Log("Batería cargada agregada a colectables del jugador");
                }
            }
        }
        else if (other.CompareTag("Player"))
        {
            if (chargingPromptPanel != null)
                chargingPromptPanel.SetActive(true);

            if (chargingPromptText != null)
            {
                if (currentBattery != null && currentBattery.isCharged)
                    chargingPromptText.text = "Presiona R para recoger la batería cargada";
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

    public void HideChargingText()
    {
        if (chargingPromptPanel != null)
            chargingPromptPanel.SetActive(false);
    }

    // Método para cuando la batería está cargada y lista para recoger
    public void BatteryReadyForPickup()
    {
        if (currentBattery != null)
        {
            // Reactivar la física
            Rigidbody rb = currentBattery.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Quitar el flag de movimiento
            currentBattery.isBeingMoved = false;

            // Agregar la batería a la lista de colectables del jugador
            if (_playerScript != null && !_playerScript.colectables.Contains(currentBattery.gameObject))
            {
                _playerScript.colectables.Add(currentBattery.gameObject);
                Debug.Log("Batería cargada agregada a colectables del jugador");
            }
        }
    }
}