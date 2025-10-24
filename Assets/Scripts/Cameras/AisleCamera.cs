using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AisleCamera : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook myVirtualCamera;
    [Tooltip("Seleccioná aquí el layer del player (por ejemplo: Player)")]
    [SerializeField] private LayerMask playerLayer; // seleccionar el layer del player en el Inspector

    private void Start()
    {
        if (myVirtualCamera != null)
            myVirtualCamera.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // comprobamos que el layer del 'other' esté dentro del LayerMask seleccionado
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        if (myVirtualCamera != null)
            myVirtualCamera.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        // misma comprobación al salir
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        if (myVirtualCamera != null)
            myVirtualCamera.gameObject.SetActive(false);
    }
}
