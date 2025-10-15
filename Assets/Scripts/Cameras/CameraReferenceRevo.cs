using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReferenceRevo : MonoBehaviour
{
    void Start()
    {
        PlayerMovement myPlayer = GameReference.Instance.player.GetComponent<PlayerMovement>();
        if (myPlayer != null)
        {
            var camera = GetComponent<CinemachineFreeLook>();
            camera.Follow = myPlayer.puntoCamaraFBX;
        }
    }
}
