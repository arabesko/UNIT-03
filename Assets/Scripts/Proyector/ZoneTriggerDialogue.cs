using UnityEngine;

public class ZoneTriggerDialogue : MonoBehaviour
{
    public string dialogueId; // el id que pusiste en ProjectorDialogue
    public ProjectorController projectorController;
    public bool onlyOnce = true; // si true, el trigger se desactiva después de usarlo

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && onlyOnce) return;

        if (other.CompareTag("Player"))
        {
            if (projectorController != null)
                projectorController.TriggerDialogueById(dialogueId);

            if (onlyOnce)
            {
                hasTriggered = true;
                // opciones: desactivar collider, desactivar gameobject, o desactivar este script
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                // gameObject.SetActive(false);
                // this.enabled = false;
            }
        }
    }

    // Método público para resetear desde otros scripts (por ej. reiniciar nivel)
    public void ResetTrigger()
    {
        hasTriggered = false;
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }
}
