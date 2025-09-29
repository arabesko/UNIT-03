using UnityEngine;
using System.Collections;

[ExecuteInEditMode]  // ¡IMPORTANTE! Esto permite que el script se ejecute en el editor
[RequireComponent(typeof(Renderer))]
public class IrisController : MonoBehaviour
{
    [Header("Material y Parámetros del Shader")]
    [Tooltip("Material que usa tu shader SimpleIris (con _IrisOffset y _IrisRadius)")]
    public Material irisMaterial;

    [Header("Offset Base (control manual de mirada)")]
    [Tooltip("Posición base del iris en UV (-1..1)")]
    public Vector2 baseOffset = Vector2.zero;

    [Header("Movimiento Aleatorio")]
    [Tooltip("Desplazamiento máximo desde la posición base (en UV)")]
    [Range(0f, 0.5f)]
    public float jitterRange = 0.05f;
    public float minMoveTime = 0.5f;
    public float maxMoveTime = 2f;

    [Header("Parpadeo")]
    public float blinkCloseTime = 0.1f;
    public float blinkOpenTime = 0.2f;
    public Vector2 blinkInterval = new Vector2(3f, 7f);
    [Tooltip("Radio abierto del iris (UV)")]
    public float openRadius = 0.2f;
    [Tooltip("Radio cerrado (casi 0)")]
    public float closedRadius = 0.01f;

    // Variables para controlar cuándo estamos en modo edición
    private bool isInEditMode = true;

    private void Awake()
    {
        // Detectar si estamos en el editor pero NO en Play Mode
        isInEditMode = !Application.isPlaying;

        if (irisMaterial == null)
            irisMaterial = GetComponent<Renderer>()?.material;
    }

    private void OnEnable()
    {
        if (irisMaterial == null)
            irisMaterial = GetComponent<Renderer>()?.material;

        // SIEMPRE aplicar los valores del inspector al material
        ApplyInspectorValuesToMaterial();

        // Solo ejecutar las corrutinas durante el Play Mode
        if (Application.isPlaying)
        {
            isInEditMode = false;
            StopAllCoroutines();
            StartCoroutine(IrisRoutine());
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            StopAllCoroutines();
        }
    }

    // Este método se llama automáticamente cuando cambias valores en el inspector
    private void OnValidate()
    {
        // Aplicar los valores inmediatamente cuando se modifican en el inspector
        if (irisMaterial != null)
        {
            ApplyInspectorValuesToMaterial();
        }
    }

    // Método crucial: aplica los valores del inspector al material
    private void ApplyInspectorValuesToMaterial()
    {
        if (irisMaterial != null)
        {
            irisMaterial.SetVector("_IrisOffset", baseOffset);
            irisMaterial.SetFloat("_IrisRadius", openRadius);
        }
    }

    // Update se ejecuta también en el editor gracias a [ExecuteInEditMode]
    private void Update()
    {
        // Si estamos en modo edición, mantener aplicados los valores del inspector
        if (isInEditMode && irisMaterial != null)
        {
            ApplyInspectorValuesToMaterial();
        }
    }

    private IEnumerator IrisRoutine()
    {
        // Pequeño delay al inicio
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (true)
        {
            // 1) Movimiento alcance jitter alrededor de baseOffset
            Vector2 start = irisMaterial.GetVector("_IrisOffset");
            Vector2 target = baseOffset + Random.insideUnitCircle * jitterRange;
            // Limitar target para que no salga del rango UV [-1,1]
            target.x = Mathf.Clamp(target.x, -1f + openRadius, 1f - openRadius);
            target.y = Mathf.Clamp(target.y, -1f + openRadius, 1f - openRadius);
            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            yield return StartCoroutine(LerpOffset(start, target, moveTime));

            // 2) Espera aleatoria antes del parpadeo
            yield return new WaitForSeconds(Random.Range(blinkInterval.x, blinkInterval.y));

            // 3) Parpadeo: cierra y abre
            yield return StartCoroutine(LerpRadius(openRadius, closedRadius, blinkCloseTime));
            yield return StartCoroutine(LerpRadius(closedRadius, openRadius, blinkOpenTime));
        }
    }

    private IEnumerator LerpOffset(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            irisMaterial.SetVector("_IrisOffset", Vector2.Lerp(from, to, t));
            yield return null;
        }
        irisMaterial.SetVector("_IrisOffset", to);
    }

    private IEnumerator LerpRadius(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            irisMaterial.SetFloat("_IrisRadius", Mathf.Lerp(from, to, t));
            yield return null;
        }
        irisMaterial.SetFloat("_IrisRadius", to);
    }
}