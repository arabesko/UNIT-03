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
    [SerializeField] private float batterySpeed = 5f;
    [SerializeField] private float batteryRotationSpeed = 5f;
    [SerializeField] private float offsetY = -90f;

    [Header("Object Movement on Charged")]
    [SerializeField] private Transform objectToMove; // Objeto que se moverá cuando la batería esté cargada
    [SerializeField] private Transform objectTargetPosition; // Posición objetivo del objeto
    [SerializeField] private float objectMoveSpeed = 2f; // Velocidad de movimiento del objeto
    private bool shouldMoveObject = false; // Controla si el objeto debe moverse

    private PortableBattery currentBattery;
    private bool isMovingBattery = false;
    private int currentWaypointIndex = 0;
    private bool isMovementCompleted = false;

    // Referencia al script del jugador
    private PlayerMovement _playerScript;

    private void Start()
    {
        _playerScript = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {
        if (isMovingBattery && currentBattery != null)
        {
            MoveBatteryAlongWaypoints();
        }

        // Mover el objeto si está activado
        if (shouldMoveObject && objectToMove != null && objectTargetPosition != null)
        {
            MoveObject();
        }
    }

    private void MoveBatteryAlongWaypoints()
    {
        if (currentWaypointIndex >= waypoints.Count || waypoints.Count == 0)
        {
            Debug.Log("Movimiento completado, iniciando carga");
            isMovingBattery = false;
            isMovementCompleted = true;

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

        // Movimiento directo como en ElevatorPower
        Vector3 direction = (target.position - currentBattery.transform.position).normalized;
        currentBattery.transform.position += direction * batterySpeed * Time.deltaTime;

        RotateBatteryTowards(target.position);

        if (Vector3.Distance(currentBattery.transform.position, target.position) < 0.2f)
        {
            currentWaypointIndex++;
            Debug.Log("Pasando al siguiente waypoint: " + currentWaypointIndex);
        }
    }

    private void RotateBatteryTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - currentBattery.transform.position);
        direction.y = 0; // Ignorar componente Y para rotación

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, offsetY, 0);

        currentBattery.transform.rotation = Quaternion.Slerp(
            currentBattery.transform.rotation,
            correctedRotation,
            batteryRotationSpeed * Time.deltaTime
        );
    }

    // Método para mover el objeto suavemente hacia la posición objetivo
    private void MoveObject()
    {
        Vector3 direction = (objectTargetPosition.position - objectToMove.position).normalized;
        objectToMove.position += direction * objectMoveSpeed * Time.deltaTime;

        // Si está muy cerca, detenemos el movimiento
        if (Vector3.Distance(objectToMove.position, objectTargetPosition.position) < 0.01f)
        {
            shouldMoveObject = false;
            Debug.Log("Objeto ha llegado a su posición final");
        }
    }

    // En el método BatteryReadyForPickup, activamos el movimiento del objeto
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

            // Activamos el movimiento del objeto
            shouldMoveObject = true;
        }
    }

    // Resto del código (OnTriggerEnter, OnTriggerExit, HideChargingText) sin cambios...
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
}