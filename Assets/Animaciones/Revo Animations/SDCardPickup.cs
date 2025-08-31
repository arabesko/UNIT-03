using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SDCardPickup : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var switcher = FindObjectOfType<PlayerMovement>();
        if (switcher != null)
        {

            //Agregar Inventario
        
        }

        Destroy(gameObject);
    }
}
