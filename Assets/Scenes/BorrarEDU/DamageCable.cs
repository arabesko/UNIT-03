using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCable : MonoBehaviour
{
    [SerializeField] float _damage;
    [SerializeField] PlayerMovement _playerMovement;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            print("Colision player");
            playerMovement.Damage(_damage);
        }
    }
}
