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
    private Vector3[] originalPositions; // Almacenar posiciones originales

    [Header("Sonidos de puerta")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip; // Nuevo sonido de cierre

    [Header("Sonido de cronómetro")]
    [SerializeField] private AudioSource timerAudioSource;
    [SerializeField] private AudioClip timerClip;

    [Header("Temporizador de cierre")]
    [SerializeField] private float closeDelay = 10f; // Tiempo editable en segundos

    private Coroutine closeCoroutine;
    private bool timerSoundPlaying = false;

    private void Start()
    {
        // Guardar posiciones originales de las puertas
        originalPositions = new Vector3[doorsToOpen.Length];
        for (int i = 0; i < doorsToOpen.Length; i++)
        {
            originalPositions[i] = doorsToOpen[i].position;
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !opened)
        {
            OpenDoors();
        }
    }

    private void OpenDoors()
    {
        opened = true;

        // Sonido de abrir puerta
        if (audioSource != null && openDoorClip != null)
        {
            audioSource.PlayOneShot(openDoorClip);
        }

        // Iniciar sonido del cronómetro
        StartTimerSound();

        // Mover puertas
        foreach (Transform door in doorsToOpen)
        {
            StartCoroutine(MoveDoorUp(door));
        }

        // Iniciar temporizador de cierre
        if (closeCoroutine != null) StopCoroutine(closeCoroutine);
        closeCoroutine = StartCoroutine(CloseDoorsAfterDelay());
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

    IEnumerator MoveDoorDown(Transform door, int doorIndex)
    {
        Vector3 targetPos = originalPositions[doorIndex];
        while (Vector3.Distance(door.position, targetPos) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator CloseDoorsAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay); // Usa la variable editable

        CloseDoors();
    }

    private void CloseDoors()
    {
        // Detener sonido del cronómetro
        StopTimerSound();

        // Sonido de cerrar puerta
        if (audioSource != null && closeDoorClip != null)
        {
            audioSource.PlayOneShot(closeDoorClip);
        }

        // Mover puertas a su posición original
        for (int i = 0; i < doorsToOpen.Length; i++)
        {
            StartCoroutine(MoveDoorDown(doorsToOpen[i], i));
        }

        opened = false;
    }

    private void StartTimerSound()
    {
        if (timerAudioSource != null && timerClip != null && !timerSoundPlaying)
        {
            timerAudioSource.clip = timerClip;
            timerAudioSource.Play();
            timerSoundPlaying = true;
        }
    }

    private void StopTimerSound()
    {
        if (timerAudioSource != null && timerSoundPlaying)
        {
            timerAudioSource.Stop();
            timerSoundPlaying = false;
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
}