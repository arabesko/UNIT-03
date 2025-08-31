using System.Collections;
using UnityEngine;

public class BridgeSpawner : MonoBehaviour
{
    [Header("Bridge Settings")]
    public GameObject bridgePrefab;
    public Transform spawnPoint;
    public float bridgeDuration = 4f;
    public float dissolveSpeed = 1f; // Velocidad de aparición y desaparición

    private bool playerInRange = false;
    private bool isSpawning = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isSpawning)
        {
            StartCoroutine(SpawnBridge());
        }
    }

    private IEnumerator SpawnBridge()
    {
        isSpawning = true;

        // Instanciar el puente
        GameObject bridge = Instantiate(bridgePrefab, spawnPoint.position, spawnPoint.rotation);

        // Obtener todos los renderers en hijos (incluye el objeto raíz)
        Renderer[] renderers = bridge.GetComponentsInChildren<Renderer>();

        // Inicializar todos los materiales con _Disolve en 0
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.SetFloat("_Disolve", 0f);
            }
        }

        // Aparición progresiva
        float value = 0f;
        while (value < 1f)
        {
            value += Time.deltaTime * dissolveSpeed;
            foreach (Renderer r in renderers)
            {
                foreach (Material m in r.materials)
                {
                    m.SetFloat("_Disolve", Mathf.Clamp01(value));
                }
            }
            yield return null;
        }

        // Mantener activo el puente
        yield return new WaitForSeconds(bridgeDuration);

        // Desaparición progresiva
        value = 1f;
        while (value > 0f)
        {
            value -= Time.deltaTime * dissolveSpeed;
            foreach (Renderer r in renderers)
            {
                foreach (Material m in r.materials)
                {
                    m.SetFloat("_Disolve", Mathf.Clamp01(value));
                }
            }
            yield return null;
        }

        // Destruir puente
        Destroy(bridge);
        isSpawning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}