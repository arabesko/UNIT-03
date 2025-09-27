using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AisleCamera : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook myVirtualCamera;
    [SerializeField] private CinemachineVirtualCameraBase myVirtualCameraBase;

    private void Start()
    {
        myVirtualCamera.gameObject.SetActive(false);
        myVirtualCameraBase.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        myVirtualCamera.gameObject.SetActive(true);
        myVirtualCameraBase.gameObject.SetActive(true);

    }

    private void OnTriggerExit(Collider other)
    {
        myVirtualCamera.gameObject.SetActive(false);
        myVirtualCameraBase.gameObject.SetActive(false);
    }

}
