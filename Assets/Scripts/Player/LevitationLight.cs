using System.Collections;
using UnityEngine;

// Asegura que se ejecute después del PlayerMovement para leer ElementLevitated/ElementDetected actualizados.
[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
public class LevitationLight : MonoBehaviour
{
    [Header("Asignar en Inspector")]
    public PlayerMovement player;            // tu PlayerMovement
    public Transform originTransform;        // ej: _projectorPosition (donde la luz queda pegada al player)
    [Tooltip("Prefab que contiene la Light tal como la configuraste (Spot/Point, Intensity, Color, etc).")]
    public GameObject lightPrefab;

    [Header("Comportamiento")]
    [Tooltip("Si true: la luz se enciende cuando hay objeto detectado (además de cuando está levitado).")]
    public bool showWhenDetected = true;
    [Tooltip("Si true: la luz solo se enciende cuando HAY LEVITADO (ignora 'detected').")]
    public bool onlyWhenLevitated = false;
    [Tooltip("Factor aplicado a la intensidad original para el modo 'detectado' (ej 0.35 = 35%).")]
    [Range(0f, 1f)]
    public float dimFactor = 0.35f;
    public float fadeDuration = 0.14f;

    // runtime
    GameObject runtimeLightGO;
    Light runtimeLight;
    float originalIntensity = 1f;
    Coroutine fadeCoroutine;

    void Start()
    {
        if (player == null || originTransform == null || lightPrefab == null)
        {
            Debug.LogWarning("LevitationLight: asigná player, originTransform y lightPrefab en el Inspector.");
            enabled = false;
            return;
        }

        // Instanciá el prefab y lo parentás al originTransform PARA QUE SIEMPRE QUEDE PEGADO AL PLAYER.
        runtimeLightGO = Instantiate(lightPrefab, originTransform.position, originTransform.rotation, originTransform);
        runtimeLightGO.name = "LevitationLight_Runtime";
        runtimeLightGO.transform.localPosition = Vector3.zero;
        runtimeLightGO.transform.localRotation = Quaternion.identity;

        // Buscá la Light dentro del prefab (root o hijos).
        runtimeLight = runtimeLightGO.GetComponentInChildren<Light>();
        if (runtimeLight == null)
        {
            Debug.LogWarning("LevitationLight: el prefab no tiene componente Light en root ni en hijos.");
            enabled = false;
            return;
        }

        // Guardamos la intensidad original para respetar tu configuración (el script solo la lerpea).
        originalIntensity = runtimeLight.intensity;

        // Empezamos apagada
        runtimeLight.intensity = 0f;
    }

    void LateUpdate()
    {
        if (runtimeLight == null || player == null) return;

        GameObject levitated = player.ElementLevitated;
        GameObject detected = player.ElementDetected;

        GameObject target = null;

        if (levitated != null)
        {
            target = levitated;
        }
        else if (!onlyWhenLevitated && showWhenDetected && detected != null)
        {
            target = detected;
        }

        if (target != null)
        {
            // La luz está parentada al originTransform (player). Rotamos el runtimeLightGO para mirar al target mundial.
            Vector3 targetWorldPos = target.transform.position;
            runtimeLightGO.transform.LookAt(targetWorldPos);

            // intensidad: full si está levitado, dim si solo detectado
            float desired = (levitated != null) ? originalIntensity : originalIntensity * dimFactor;
            StartFadeIntensity(desired);
        }
        else
        {
            // no hay target: apagamos la luz
            StartFadeIntensity(0f);
        }
    }

    void OnDisable()
    {
        if (runtimeLight != null) runtimeLight.intensity = 0f;
    }

    void StartFadeIntensity(float target)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIntensityCoroutine(target, fadeDuration));
    }

    IEnumerator FadeIntensityCoroutine(float target, float duration)
    {
        if (runtimeLight == null) yield break;
        float start = runtimeLight.intensity;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, duration));
            runtimeLight.intensity = Mathf.Lerp(start, target, t);
            yield return null;
        }
        runtimeLight.intensity = target;
        fadeCoroutine = null;
    }

    // Fuerza apagar instantáneamente
    public void ForceOffInstant()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (runtimeLight != null) runtimeLight.intensity = 0f;
    }
}
