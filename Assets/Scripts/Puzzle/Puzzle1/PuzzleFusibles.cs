using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class PuzzleFusibles : MonoBehaviour
{
    [System.Serializable]
    public class FuseAssignment
    {
        public string fuseID;
        public Transform slotTransform;
        public Transform placementPoint;
        public bool isOccupied = false;
        public List<Transform> PointsMovement = new List<Transform>();
    }

    [Header("Configuración General")]
    public List<FuseAssignment> fuseAssignments = new List<FuseAssignment>();
    public TMP_Text percentText;

    [Header("Comportamiento de Puerta")]
    public bool enableDoor = true;
    public Transform door;
    public Transform doorOpenPosition;

    [Header("Comportamiento de Luces")]
    public bool enableLights = true;
    public List<Light> successLights;

    [Header("Audio - Puerta")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenSound;

    [Header("Audio - Fusibles")]
    public AudioSource fuseAudioSource;
    public AudioClip fuseInsertSound;

    [Header("Configuración General")]
    public float openSpeed = 1f;

    [Header("Eventos")]
    public UnityEvent onPuzzleComplete; // Evento que se dispara cuando se completa al 100%

    private Dictionary<string, FuseAssignment> fuseDictionary = new Dictionary<string, FuseAssignment>();
    private List<GameObject> insertedFuses = new List<GameObject>();
    private int totalPercent = 0;
    private bool isDoorOpen = false;
    private bool isPuzzleComplete = false;

    [SerializeField] private int _fusibleSpeed;
    [SerializeField] private int _fusibleSpeedRotation;
    [SerializeField] private float offsetY = -90f;

    // Propiedad pública para verificar si el puzzle está completo
    public bool IsPuzzleComplete => isPuzzleComplete;
    public int TotalPercent => totalPercent;

    private void Start()
    {
        foreach (FuseAssignment assignment in fuseAssignments)
        {
            if (!fuseDictionary.ContainsKey(assignment.fuseID))
            {
                fuseDictionary.Add(assignment.fuseID, assignment);
            }
        }

        if (enableLights)
        {
            InitializeLights();
        }
    }

    private void InitializeLights()
    {
        SetLightsColor(successLights, Color.red);
    }

    private void SetLightsColor(List<Light> lights, Color color)
    {
        foreach (Light light in lights)
        {
            if (light != null)
            {
                light.color = color;
                light.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDoorOpen) return;

        ElementPuzzle fuse = other.GetComponent<ElementPuzzle>();
        if (fuse == null || insertedFuses.Contains(fuse.gameObject)) return;

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

        if (fuse._player != null && fuse._player.colectables.Contains(fuse.gameObject))
        {
            fuse._player.colectables.Remove(fuse.gameObject);
            fuse._player.NoLevitate();
        }

        Rigidbody rb = fuse.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider fuseCollider = fuse.GetComponent<Collider>();
        if (fuseCollider != null) fuseCollider.enabled = false;

        if (assignment.placementPoint != null)
        {
            StartCoroutine(MoveBatteryToPosition(fuse.transform, assignment.PointsMovement, 0, fuse));
        }
        else
        {
            fuse.transform.localPosition = Vector3.zero;
            fuse.transform.localRotation = Quaternion.identity;

            totalPercent += fuse.MyReturnNumber();
            percentText.text = totalPercent.ToString() + "%";
            CheckCompletion();
        }

        fuse.transform.localScale = Vector3.one;
        fuse.gameObject.layer = 0;

        InteractableUI interactableUI = fuse.GetComponent<InteractableUI>();
        if (interactableUI != null) Destroy(interactableUI);

        if (fuseCollider != null) fuseCollider.enabled = true;
        fuse.Desactivate();
    }

    public IEnumerator MoveBatteryToPosition(Transform fusible, List<Transform> points, int index, ElementPuzzle fuse)
    {
        bool swFin = false;
        Vector3 dir = (points[index].transform.position - fusible.transform.position).normalized;
        while (swFin == false)
        {
            fusible.transform.position += dir * _fusibleSpeed * Time.deltaTime;
            if (Vector3.Distance(fusible.transform.position, points[index].transform.position) < 0.2f)
            {
                index++;
                if (index >= points.Count)
                {
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

        // Usar el AudioSource específico para fusibles
        if (fuseAudioSource != null && fuseInsertSound != null)
        {
            fuseAudioSource.PlayOneShot(fuseInsertSound);
        }

        totalPercent += fuse.MyReturnNumber();
        percentText.text = totalPercent.ToString() + "%";
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (totalPercent >= 100 && !isPuzzleComplete)
        {
            isPuzzleComplete = true;

            // Disparar evento
            onPuzzleComplete?.Invoke();

            if (enableLights)
            {
                SetLightsColor(successLights, Color.green);
            }

            if (enableDoor)
            {
                StartCoroutine(OpenDoor());
                isDoorOpen = true;
            }
        }
    }

    private IEnumerator OpenDoor()
    {
        // Usar el AudioSource específico para la puerta
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