using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChargingStation : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject chargingPromptPanel;
    [SerializeField] private TextMeshProUGUI chargingPromptText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null && !battery.isCharged)
            {
                battery.StartCharging(this); // Le avisamos al script de la batería que inicie la carga

                if (chargingPromptPanel != null)
                    chargingPromptPanel.SetActive(true);

                if (chargingPromptText != null)
                    chargingPromptText.text = "Cargando bateria...";
            }
        }
        else if (other.CompareTag("Player"))
        {
            if (chargingPromptPanel != null)
                chargingPromptPanel.SetActive(true);

            if (chargingPromptText != null)
                chargingPromptText.text = "Parece una fuente de energia.";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Battery"))
        {
            PortableBattery battery = other.GetComponent<PortableBattery>();
            if (battery != null)
            {
                battery.StopCharging(); // Si la batería sale del trigger antes de tiempo, detenemos la carga
            }
        }

        // Ocultar el panel si se va el jugador o la batería
        if (other.CompareTag("Player") || other.CompareTag("Battery"))
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