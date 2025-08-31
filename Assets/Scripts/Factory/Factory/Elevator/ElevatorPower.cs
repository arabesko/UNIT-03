using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ElevatorPower : MonoBehaviour
{
    [Header("Power Settings")]
    [SerializeField] private Light statusLight;
    [SerializeField] private Light secondaryStatusLight;

    [Header("Battery Installation")]
    [SerializeField] private Transform _batteryBox;
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField] private float _speedBoxBattery;
    [SerializeField] private List<Transform> _points;
    [SerializeField] private GameObject _battery;
    [SerializeField] private Rigidbody _rbBattery;
    [SerializeField] private float _batterySpeed = 5;
    [SerializeField] private float _batterySpeedRotation = 5;
    [SerializeField] private float offsetY = -90f;

    private bool _isInstalling = false;
    private int _indexBattery = 0;
    private bool _powerActivated = false;
    private PlayerMovement _playerScript;

    public bool HasPower => _powerActivated;

    private void Start()
    {
        if (statusLight != null)
            statusLight.color = Color.red;

        if (secondaryStatusLight != null)
            secondaryStatusLight.color = Color.red;

        // Encontrar el jugador si no está asignado
        if (_playerScript == null)
            _playerScript = FindObjectOfType<PlayerMovement>();
    }

    private void Update()
    {
        if (_isInstalling)
        {
            MoveBatteryToPosition();
        }
    }

    private void MoveBatteryToPosition()
    {
        Vector3 dir = (_points[_indexBattery].transform.position - _battery.transform.position).normalized;
        _battery.transform.position += dir * _batterySpeed * Time.deltaTime;
        RotateTowards(_points[_indexBattery].transform.position);

        if (Vector3.Distance(_battery.transform.position, _points[_indexBattery].transform.position) < 0.2f)
        {
            _indexBattery++;
        }

        if (_indexBattery >= _points.Count)
        {
            _isInstalling = false;
            _battery.transform.SetParent(_batteryBox);
            StartCoroutine(OpenCloseBoxBattery(_pointA));
            ActivatePower();
        }
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - _battery.transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, offsetY, 0);

        _battery.transform.rotation = Quaternion.Slerp(_battery.transform.rotation, correctedRotation,
                                              _batterySpeedRotation * Time.deltaTime);
    }

    private void ActivatePower()
    {
        _powerActivated = true;
        Debug.Log("Batería instalada, elevador con energía.");

        if (statusLight != null)
            statusLight.color = Color.green;

        if (secondaryStatusLight != null)
            secondaryStatusLight.color = Color.green;

        ElevatorPlayerDetector detector = FindObjectOfType<ElevatorPlayerDetector>();
        if (detector != null)
        {
            detector.OnPowerActivated();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null && battery.isCharged)
            {
                //var UIBattery = other.GetComponent<InteractableText>();
                //if (UIBattery != null)
                //    UIBattery._isUIActivate = false;

                StartCoroutine(OpenCloseBoxBattery(_pointB));
                if (_playerScript != null)
                {
                    _playerScript.colectables.Remove(_battery);
                    _playerScript.NoLevitate();
                }

                _rbBattery.isKinematic = true;
                _isInstalling = true;
                _indexBattery = 0; // Resetear índice
            }
        }
    }

    private IEnumerator OpenCloseBoxBattery(Transform point)
    {
        while (Vector3.Distance(_batteryBox.position, point.position) > 0.2f)
        {
            var dir = (point.position - _batteryBox.position).normalized;
            _batteryBox.position += dir * _speedBoxBattery * Time.deltaTime;
            yield return null;
        }
    }
}