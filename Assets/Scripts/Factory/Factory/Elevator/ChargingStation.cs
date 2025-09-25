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
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform objectTargetPosition;
    [SerializeField] private float objectMoveSpeed = 2f;
    private bool shouldMoveObject = false;

    [Header("Charging Lights")]
    [SerializeField] private Light light1; // Point Light para la primera luz
    [SerializeField] private Light light2; // Point Light para la segunda luz
    [SerializeField] private Light light3; // Point Light para la tercera luz
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private float lightIntensity = 2f; // Intensidad de las luces
    private bool[] lightsStatus = new bool[3]; // false = rojo, true = verde

    private PortableBattery currentBattery;
    private bool isMovingBattery = false;
    private int currentWaypointIndex = 0;
    private bool isMovementCompleted = false;
    private Coroutine chargingLightsCoroutine;

    // Referencia al script del jugador
    private PlayerMovement _playerScript;

    private void Start()
    {
        _playerScript = FindObjectOfType<PlayerMovement>();
        InitializeLights();
    }

    private void Update()
    {
        if (isMovingBattery && currentBattery != null)
        {
            MoveBatteryAlongWaypoints();
        }

        if (shouldMoveObject && objectToMove != null && objectTargetPosition != null)
        {
            MoveObject();
        }
    }

    // Inicializar todas las luces en rojo
    private void InitializeLights()
    {
        SetLightColor(light1, redColor);
        SetLightColor(light2, redColor);
        SetLightColor(light3, redColor);

        lightsStatus[0] = false;
        lightsStatus[1] = false;
        lightsStatus[2] = false;
    }

    // Método para cambiar el color de una luz
    private void SetLightColor(Light light, Color color)
    {
        if (light != null)
        {
            light.color = color;
            light.intensity = lightIntensity;
        }
    }

    // Corrutina para controlar el cambio de luces
    private IEnumerator ChargingLightsSequence()
    {
        // Luz 1 se enciende después de 1 segundo
        yield return new WaitForSeconds(1f);
        SetLightColor(light1, greenColor);
        lightsStatus[0] = true;

        // Luz 2 se enciende después de 2 segundos
        yield return new WaitForSeconds(1f);
        SetLightColor(light2, greenColor);
        lightsStatus[1] = true;

        // Luz 3 se enciende después de 3 segundos
        yield return new WaitForSeconds(0.5f);
        SetLightColor(light3, greenColor);
        lightsStatus[2] = true;

        Debug.Log("Todas las luces están verdes - Carga completa");
    }

    // Iniciar la secuencia de carga de luces
    private void StartChargingLights()
    {
        // Reiniciar luces a rojo antes de comenzar
        InitializeLights();

        if (chargingLightsCoroutine != null)
            StopCoroutine(chargingLightsCoroutine);

        chargingLightsCoroutine = StartCoroutine(ChargingLightsSequence());
    }

    // Detener la secuencia de luces
    private void StopChargingLights()
    {
        if (chargingLightsCoroutine != null)
        {
            StopCoroutine(chargingLightsCoroutine);
            chargingLightsCoroutine = null;
        }
    }

    // Método para resetear las luces cuando se retira la batería
    public void ResetLights()
    {
        StopChargingLights();
        InitializeLights();
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

                // Iniciar la secuencia de luces cuando comienza la carga
                StartChargingLights();

                if (chargingPromptPanel != null)
                    chargingPromptPanel.SetActive(true);

                if (chargingPromptText != null)
                    chargingPromptText.text = "Cargando bateria...";
            }
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
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
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, offsetY, 0);

        currentBattery.transform.rotation = Quaternion.Slerp(
            currentBattery.transform.rotation,
            correctedRotation,
            batteryRotationSpeed * Time.deltaTime
        );
    }

    private void MoveObject()
    {
        Vector3 direction = (objectTargetPosition.position - objectToMove.position).normalized;
        objectToMove.position += direction * objectMoveSpeed * Time.deltaTime;

        if (Vector3.Distance(objectToMove.position, objectTargetPosition.position) < 0.01f)
        {
            shouldMoveObject = false;
            Debug.Log("Objeto ha llegado a su posición final");
        }
    }

    public void BatteryReadyForPickup()
    {
        if (currentBattery != null)
        {
            Rigidbody rb = currentBattery.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            currentBattery.isBeingMoved = false;

            if (_playerScript != null && !_playerScript.colectables.Contains(currentBattery.gameObject))
            {
                _playerScript.colectables.Add(currentBattery.gameObject);
                Debug.Log("Batería cargada agregada a colectables del jugador");
            }

            shouldMoveObject = true;

            // Las luces se mantienen verdes hasta que se retira la batería
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

                if (_playerScript != null)
                {
                    if (_playerScript.colectables.Contains(battery.gameObject))
                    {
                        _playerScript.colectables.Remove(battery.gameObject);
                        _playerScript.NoLevitate();
                        Debug.Log("Batería removida del control del jugador");
                    }
                }

                isMovingBattery = true;
                currentBattery = battery;
                currentWaypointIndex = 0;

                Rigidbody rb = currentBattery.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                currentBattery.isBeingMoved = true;

                // Resetear luces si había una carga previa
                //ResetLights();
            }
            else if (battery != null && battery.isCharged)
            {
                if (_playerScript != null && !_playerScript.colectables.Contains(battery.gameObject))
                {
                    _playerScript.colectables.Add(battery.gameObject);
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
        // Cuando la batería cargada sale del trigger, resetear las luces
        else if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null && battery.isCharged)
            {
                //ResetLights();
                isMovementCompleted = false; // Permitir nueva carga
            }
        }
    }

    public void HideChargingText()
    {
        if (chargingPromptPanel != null)
            chargingPromptPanel.SetActive(false);
    }
}