using UnityEngine;
using System.Collections;

public class FlickerGlitch : MonoBehaviour
{
    [Header("Targets Sequence")]
    public GameObject[] targets;
    public GameObject lightObject;
    public ParticleSystem particleSystem;

    [Header("Audio Clips")]
    public AudioClip glitchSound;
    public AudioClip onSound;

    [Header("Trigger Key & Cooldown")]
    public KeyCode triggerKey = KeyCode.E;
    public float keyCooldown = 3f;

    [Header("Flicker Settings")]
    public float flickerDurationOn = 0.5f;
    public int flickerCountOn = 6;
    public float onDuration = 2f;
    public float flickerDurationOff = 0.3f;
    public int flickerCountOff = 4;

    private bool isRunning = false;
    private float nextAvailableTime = 0f;
    private AudioSource audioSource;
    private int currentIdx = -1;

    // NUEVO: para detectar si el jugador está en rango
    private bool isPlayerInRange = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;

        if (targets != null)
        {
            foreach (var go in targets)
                if (go) go.SetActive(false);
        }
        if (lightObject)
            lightObject.SetActive(false);
        if (particleSystem)
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        if (lightObject != null && isRunning && currentIdx >= 0 && currentIdx < targets.Length)
        {
            var tgt = targets[currentIdx];
            if (tgt != null)
                lightObject.transform.LookAt(tgt.transform.position);
        }

        // SOLO si el jugador está en rango se puede activar con E
        if (isPlayerInRange && Input.GetKeyDown(triggerKey) && Time.time >= nextAvailableTime && !isRunning)
        {
            StartCoroutine(RunSequence());
        }
    }

    IEnumerator RunSequence()
    {
        isRunning = true;
        nextAvailableTime = Time.time + keyCooldown;

        if (particleSystem)
            particleSystem.Play();

        for (int i = 0; i < targets.Length; i++)
        {
            currentIdx = i;
            var go = targets[i];
            if (go == null) continue;

            audioSource.Stop();

            if (glitchSound != null)
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(glitchSound);
            }

            yield return StartCoroutine(Flicker(go, lightObject, true, flickerCountOn, flickerDurationOn));

            SetState(go, true);
            if (lightObject) SetState(lightObject, true);

            if (onSound != null)
            {
                audioSource.clip = onSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            yield return new WaitForSeconds(onDuration);

            if (glitchSound != null)
            {
                audioSource.loop = false;
                audioSource.PlayOneShot(glitchSound);
            }

            yield return StartCoroutine(Flicker(go, lightObject, false, flickerCountOff, flickerDurationOff));

            SetState(go, false);
            if (lightObject) SetState(lightObject, false);

            audioSource.Stop();
        }

        if (particleSystem)
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        currentIdx = -1;
        isRunning = false;
    }

    IEnumerator Flicker(GameObject go, GameObject lightGo, bool finalState, int count, float duration)
    {
        float interval = duration / (count * 2f);
        for (int i = 0; i < count * 2; i++)
        {
            bool state = (i % 2 == 0) ? finalState : !finalState;
            SetState(go, state);
            if (lightGo) SetState(lightGo, state);
            yield return new WaitForSeconds(interval);
        }
        SetState(go, finalState);
        if (lightGo) SetState(lightGo, finalState);
    }

    void SetState(GameObject go, bool on)
    {
        if (go) go.SetActive(on);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
