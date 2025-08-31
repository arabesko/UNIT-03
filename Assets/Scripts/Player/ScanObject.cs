using System.Collections;
using UnityEngine;
using TMPro;

public class ScanObject : MonoBehaviour
{
    public GameObject scanPanel;

    public bool isPlayerInRange = false;

    private void Start()
    {
        scanPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // if (!isScanning)
            scanPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            scanPanel.SetActive(false);
        }
    }
}
