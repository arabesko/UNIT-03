using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Unity.VisualScripting;

public class PuzzleFusibles : MonoBehaviour
{
    [System.Serializable]
    public class FuseAssignment
    {
        public string fuseID; // ID único del fusible
        public Transform slotTransform; // Slot asignado a este fusible
        public Transform placementPoint; // Punto de colocación exacto
        public bool isOccupied = false;
        public List<Transform> PointsMovement = new List<Transform>();
    }

    public List<FuseAssignment> fuseAssignments = new List<FuseAssignment>();
    public TMP_Text percentText;
    public Transform door;
    public Transform doorOpenPosition;

    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;
    public AudioClip fuseInsertSound;
    public float openSpeed = 1f;

    private Dictionary<string, FuseAssignment> fuseDictionary = new Dictionary<string, FuseAssignment>();
    private List<GameObject> insertedFuses = new List<GameObject>();
    private int totalPercent = 0;
    private bool isDoorOpen = false;

    [SerializeField] private int _fusibleSpeed;
    [SerializeField] private int _fusibleSpeedRotation;
    [SerializeField] private float offsetY = -90f;

    private void Start()
    {
        // Inicializar el diccionario de asignaciones
        foreach (FuseAssignment assignment in fuseAssignments)
        {
            if (!fuseDictionary.ContainsKey(assignment.fuseID))
            {
                fuseDictionary.Add(assignment.fuseID, assignment);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDoorOpen) return;

        ElementPuzzle fuse = other.GetComponent<ElementPuzzle>();
        if (fuse == null || insertedFuses.Contains(fuse.gameObject)) return;

        // Buscar la asignación específica para este fusible
        if (fuseDictionary.TryGetValue(fuse.fuseID, out FuseAssignment assignment))
        {
            if (!assignment.isOccupied)
            {
                InsertFuse(fuse, assignment);
            }
        }
    }

    private void InsertFuse(ElementPuzzle fuse, FuseAssignment assignment)
    {
        assignment.isOccupied = true;
        insertedFuses.Add(fuse.gameObject);

        // Liberar el fusible del jugador
        if (fuse._player != null && fuse._player.colectables.Contains(fuse.gameObject))
        {
            fuse._player.colectables.Remove(fuse.gameObject);
            fuse._player.NoLevitate();
        }

        // Desactivar física
        Rigidbody rb = fuse.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Desactivar collider
        Collider fuseCollider = fuse.GetComponent<Collider>();
        if (fuseCollider != null) fuseCollider.enabled = false;

        // Posicionar el fusible en su slot asignado
        //fuse.transform.SetParent(assignment.slotTransform);

        if (assignment.placementPoint != null)
        {
            StartCoroutine(MoveBatteryToPosition(fuse.transform, assignment.PointsMovement, 0));
            //fuse.transform.position = assignment.placementPoint.position;
            //fuse.transform.rotation = assignment.placementPoint.rotation;
        }
        else
        {
            fuse.transform.localPosition = Vector3.zero;
            fuse.transform.localRotation = Quaternion.identity;
        }

        fuse.transform.localScale = Vector3.one;

        // Cambiar la layer a Default
        fuse.gameObject.layer = 0; // 0 es el índice de la layer "Default"

        // Destruir el componente InteractableUI si existe
        InteractableUI interactableUI = fuse.GetComponent<InteractableUI>();
        if (interactableUI != null)
        {
            Destroy(interactableUI);
        }

        // Reactivar collider
        if (fuseCollider != null) fuseCollider.enabled = true;

        fuse.Desactivate();

        

        // Actualizar porcentaje
        totalPercent += fuse.MyReturnNumber();
        percentText.text = totalPercent.ToString() + "%";

        
    }

    public IEnumerator MoveBatteryToPosition(Transform fusible, List<Transform> points, int index)
    {
        
        bool swFin = false;
        Vector3 dir = (points[index].transform.position - fusible.transform.position).normalized;
        while (swFin == false)
        {
            fusible.transform.position += dir * _fusibleSpeed * Time.deltaTime;
            //RotateTowards(fusible.gameObject, points[index].transform.position);
            if (Vector3.Distance(fusible.transform.position, points[index].transform.position) < 0.2f)
            {
                index++;
                if (index >= points.Count)
                {
                    //Esta en la posicion final
                    swFin = true;
                }
                else
                {
                    dir = (points[index].transform.position - fusible.transform.position).normalized;
                }
            }
            yield return null;
        }
        fusible.transform.position = points[0].transform.position;
        fusible.transform.rotation = points[0].transform.rotation;

        // Verificar si el puzzle está completo
        if (totalPercent >= 100)
        {
            StartCoroutine(OpenDoor());
        }

        // Sonido
        if (doorAudioSource != null && fuseInsertSound != null)
        {
            doorAudioSource.PlayOneShot(fuseInsertSound);
        }
    }

    private void RotateTowards(GameObject fusible, Vector3 target)
    {
        Vector3 direction = (target - fusible.transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, offsetY, 0);

        fusible.transform.rotation = Quaternion.Slerp(fusible.transform.rotation, correctedRotation,
                                              _fusibleSpeedRotation * Time.deltaTime);
    }

    private IEnumerator OpenDoor()
    {
        isDoorOpen = true;

        if (doorAudioSource != null && doorOpenSound != null)
        {
            doorAudioSource.PlayOneShot(doorOpenSound);
        }

        float t = 0;
        Vector3 startPos = door.position;
        Vector3 endPos = doorOpenPosition.position;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (FuseAssignment assignment in fuseAssignments)
        {
            if (assignment.slotTransform != null)
            {
                Gizmos.color = assignment.isOccupied ? Color.red : Color.green;
                if (assignment.placementPoint != null)
                {
                    Gizmos.DrawWireCube(assignment.placementPoint.position, Vector3.one * 0.1f);
                }
                else
                {
                    Gizmos.DrawWireCube(assignment.slotTransform.position, Vector3.one * 0.1f);
                }
            }
        }
    }
}