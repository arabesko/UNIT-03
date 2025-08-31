using System.Collections;
using UnityEngine;
using TMPro;

public class DuctButton : MonoBehaviour
{
    [Header("Puertas")]
    [SerializeField] private Transform[] doorsToOpen;
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 1f;

    private bool isPlayerInRange = false;
    private bool opened = false;

    [Header("Sonido de puerta")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openDoorClip;

    private void Start()
    {
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !opened)
        {
            opened = true;

            // Sonido de abrir puerta
            if (audioSource != null && openDoorClip != null)
            {
                audioSource.PlayOneShot(openDoorClip);
            }

            foreach (Transform door in doorsToOpen)
            {
                StartCoroutine(MoveDoorUp(door));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !opened)
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    IEnumerator MoveDoorUp(Transform door)
    {
        Vector3 startPos = door.position;
        Vector3 targetPos = startPos + Vector3.up * moveDistance;
        while (Vector3.Distance(door.position, targetPos) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
