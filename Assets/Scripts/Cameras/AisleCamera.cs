using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AisleCamera : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook myVirtualCamera;

    private void Start()
    {
        myVirtualCamera.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        myVirtualCamera.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        myVirtualCamera.gameObject.SetActive(false);
    }

}
