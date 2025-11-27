using UnityEngine;
using System;


[RequireComponent(typeof(Collider))]
public class MapPieceView : MonoBehaviour
{
    public static event Action OnPieceCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Avisar al sistema
            OnPieceCollected?.Invoke();

            // Efecto visual simple al desaparecer (opcional)
            gameObject.SetActive(false);
        }
    }
}