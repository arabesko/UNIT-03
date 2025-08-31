using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePickup : MonoBehaviour
{
    private GameObject _moduleReference;
    [SerializeField] private float _rotationSpeed = 50f;


    private void Start()
    {
        // Configurar collider de tamaño fijo
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.radius = 2f;
        collider.isTrigger = true;

        // Añadir Rigidbody para física
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Forzar escala constante
        transform.localScale = Vector3.one * 0.3f;

        // Añadir renderer si no existe
        if (GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }

    public void SetModule(GameObject module)
    {
        _moduleReference = module;
        CopyVisualsFromModule(module);

        // Fijar escala y posición
        transform.localScale = Vector3.one * 0.5f;
        transform.position += Vector3.up * 0.5f;
    }

    private void CopyVisualsFromModule(GameObject module)
    {
        MeshFilter moduleMesh = module.GetComponent<MeshFilter>();
        if (moduleMesh != null)
        {
            MeshFilter pickupMesh = GetComponent<MeshFilter>();
            if (pickupMesh == null) pickupMesh = gameObject.AddComponent<MeshFilter>();
            pickupMesh.mesh = moduleMesh.sharedMesh;
        }

        MeshRenderer moduleRenderer = module.GetComponent<MeshRenderer>();
        if (moduleRenderer != null)
        {
            MeshRenderer pickupRenderer = GetComponent<MeshRenderer>();
            if (pickupRenderer == null) pickupRenderer = gameObject.AddComponent<MeshRenderer>();
            pickupRenderer.materials = moduleRenderer.sharedMaterials;
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Verificar distancia para evitar autorecolección
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < 1.5f) return;

            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && _moduleReference != null)
            {
                player.AddDroppedModule(_moduleReference);
                Destroy(gameObject);
            }
        }
    }
}
