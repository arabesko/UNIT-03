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
    public ElementPuzzle myPuzzle;

    [Header("Object Movement on Charged")]
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform objectTargetPosition;
    [SerializeField] private float objectMoveSpeed = 2f;
    private bool shouldMoveObject = false;

    [Header("Charging Lights")]
    [SerializeField] private Light light1;
    [SerializeField] private Light light2;
    [SerializeField] private Light light3;
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private float lightIntensity = 2f;
    private bool[] lightsStatus = new bool[3];

    [Header("Battery Lights")]
    [SerializeField] private Light batteryLightRed;
    [SerializeField] private Light batteryLightGreen;

    private PortableBattery currentBattery;
    private bool isMovingBattery = false;
    private int currentWaypointIndex = 0;
    private bool isMovementCompleted = false;
    private Coroutine chargingLightsCoroutine;

    // Variables para manejo de mensajes
    private bool isPlayerInTrigger = false;
    private bool isChargingInProgress = false;
    private Coroutine messageCoroutine;

    // Referencia al script del jugador
    private PlayerMovement _playerScript;

    // Mensajes predefinidos
    private const string SOURCE_MESSAGE = "Parece una fuente de energia.";
    private const string CHARGING_MESSAGE = "Cargando bateria...";
    private const string READY_MESSAGE = "Presiona R para recoger la batería cargada";

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

        // Actualizar mensaje según prioridades
        UpdateMessage();
    }

    private void UpdateMessage()
    {
        if (!isPlayerInTrigger)
        {
            HideMessage();
            return;
        }

        // Sistema de prioridades
        if (currentBattery != null && currentBattery.isCharged)
        {
            ShowMessage(READY_MESSAGE);
        }
        else if (isChargingInProgress)
        {
            ShowMessage(CHARGING_MESSAGE);
        }
        else
        {
            ShowMessage(SOURCE_MESSAGE);
        }
    }

    private void ShowMessage(string message)
    {
        if (chargingPromptPanel != null)
            chargingPromptPanel.SetActive(true);

        if (chargingPromptText != null)
            chargingPromptText.text = message;
    }

    private void HideMessage()
    {
        if (chargingPromptPanel != null)
            chargingPromptPanel.SetActive(false);
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

        batteryLightRed.gameObject.SetActive(false);
        batteryLightGreen.gameObject.SetActive(true);

        //Permito que la bateria pueda levitarse nuevamente
        if (myPuzzle != null) myPuzzle.isLevitable = true;
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
            isMovingBattery = false;
            isMovementCompleted = true;

            if (currentBattery != null && !currentBattery.isCharged)
            {
                currentBattery.StartCharging(this);
                isChargingInProgress = true; // Marcar que la carga está en progreso

                // Iniciar la secuencia de luces cuando comienza la carga
                StartChargingLights();
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
        }
    }

    private void RotateBatteryTowards(Vector3 targetPosition)
    {
        //Vector3 direction = (targetPosition - currentBattery.transform.position);
        //direction.y = 0;

        //if (direction.sqrMagnitude < 0.001f) return;

        //Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        //Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, offsetY, 0);

        //currentBattery.transform.rotation = Quaternion.Slerp(
        //    currentBattery.transform.rotation,
        //    correctedRotation,
        //    batteryRotationSpeed * Time.deltaTime
        //);
        Vector3 direction = (targetPosition - currentBattery.transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, offsetY, 0);

        currentBattery.transform.rotation = Quaternion.Slerp(currentBattery.transform.rotation, correctedRotation,
                                              batteryRotationSpeed * Time.deltaTime);
    }

    private void MoveObject()
    {
        Vector3 direction = (objectTargetPosition.position - objectToMove.position).normalized;
        objectToMove.position += direction * objectMoveSpeed * Time.deltaTime;

        if (Vector3.Distance(objectToMove.position, objectTargetPosition.position) < 0.01f)
        {
            shouldMoveObject = false;
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
            isChargingInProgress = false; // La carga ha terminado

            if (_playerScript != null && !_playerScript.colectables.Contains(currentBattery.gameObject))
            {
                _playerScript.colectables.Add(currentBattery.gameObject);
            }

            shouldMoveObject = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();

            if (battery != null && !battery.isCharged && !isMovingBattery && !isMovementCompleted)
            {
                if (_playerScript != null)
                {
                    if (_playerScript.colectables.Contains(battery.gameObject))
                    {
                        _playerScript.colectables.Remove(battery.gameObject);
                        _playerScript.NoLevitate();
                    }
                }

                isMovingBattery = true;

                //Impide que se colecte la bateria cuando esta yendo a ser cargada//
                myPuzzle = other.GetComponent<ElementPuzzle>();
                if (myPuzzle != null) myPuzzle.isLevitable = false;

                currentBattery = battery;
                currentWaypointIndex = 0;

                Rigidbody rb = currentBattery.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                currentBattery.isBeingMoved = true;
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
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            HideMessage();
        }
        // Cuando la batería cargada sale del trigger, resetear las luces
        else if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null && battery.isCharged)
            {
                isMovementCompleted = false; // Permitir nueva carga
                isChargingInProgress = false; // Resetear estado de carga
            }
        }
    }

    public void HideChargingText()
    {
        HideMessage();
    }
}