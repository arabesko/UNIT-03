using UnityEngine;

public class PlayerModuleSwitcher : MonoBehaviour
{
    [Header("Módulos (0 = Normal, 1 = Garras)")]
    public GameObject revoNormal;
    public GameObject revoGarras;

    private bool canSwitch = false;
    private int currentIndex = 0;

    // Referencia al Transform del padre para sincronizar posiciones
    private Transform parentTransform;

    void Start()
    {
        parentTransform = transform; // Transform del GameObject padre
        currentIndex = 0;
        revoNormal.SetActive(true);
        revoGarras.SetActive(false);
    }

    void Update()
    {
        if (canSwitch && Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleModule();
        }
    }

    private void ToggleModule()
    {
        // Obtener el módulo activo actual
        GameObject currentActive = currentIndex == 0 ? revoNormal : revoGarras;

        // Capturar la posición/rotación global del jugador activo
        Vector3 globalPosition = currentActive.transform.position;
        Quaternion globalRotation = currentActive.transform.rotation;

        // Desactivar el actual
        currentActive.SetActive(false);

        // Cambiar índice
        currentIndex = 1 - currentIndex;

        // Activar el nuevo módulo
        GameObject nextActive = currentIndex == 0 ? revoNormal : revoGarras;

        // Sincronizar posición/rotación global con el padre
        parentTransform.position = globalPosition;
        parentTransform.rotation = globalRotation;

        // Resetear la posición local del nuevo módulo (para que herede la posición del padre)
        nextActive.transform.localPosition = Vector3.zero;
        nextActive.transform.localRotation = Quaternion.identity;

        // Reiniciar componentes críticos (Rigidbody, CharacterController)
        ResetPhysicsComponents(nextActive);

        nextActive.SetActive(true);
    }

    private void ResetPhysicsComponents(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CharacterController cc = target.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            cc.enabled = true;
        }
    }

    public void UnlockSwitching()
    {
        canSwitch = true;
    }
}