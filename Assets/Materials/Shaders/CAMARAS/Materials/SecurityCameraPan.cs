using System.Collections;
using UnityEngine;

public class SecurityCameraPan : MonoBehaviour
{
    [Header("Asignar cámara o Transform (si está vacío usa este GameObject)")]
    [SerializeField] private Transform cameraTransform;

    [Header("Ángulos (relativos a la rotación inicial Y)")]
    [SerializeField, Tooltip("Ángulo hacia la izquierda respecto a la rotación inicial (grados)")]
    private float leftAngle = -45f;
    [SerializeField, Tooltip("Ángulo hacia la derecha respecto a la rotación inicial (grados)")]
    private float rightAngle = 45f;

    [Header("Movimiento")]
    [SerializeField, Tooltip("Velocidad en grados por segundo")]
    private float speed = 30f;
    [SerializeField, Tooltip("Tiempo que se queda en cada extremo (segundos)")]
    private float holdTime = 0.6f;
    [SerializeField, Tooltip("Si está activo, usa la curva de easing para suavizar")]
    private bool smooth = true;
    [SerializeField, Tooltip("Curva para el easing (0..1)")]
    private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Tooltip("Arranca hacia la izquierda si es true, sino hacia la derecha")]
    private bool startAtLeft = true;
    [SerializeField, Tooltip("Usar rotación local (true) o rotación mundial (false)")]
    private bool useLocalRotation = true;

    // estado interno
    private float initialY;
    private Coroutine loopCoroutine;

    void Reset()
    {
        // valor por defecto para la curva cuando se añade desde el inspector
        ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = transform;
    }

    void Start()
    {
        initialY = useLocalRotation ? cameraTransform.localEulerAngles.y : cameraTransform.eulerAngles.y;
        loopCoroutine = StartCoroutine(PanLoop());
    }

    private IEnumerator PanLoop()
    {
        float from = startAtLeft ? leftAngle : rightAngle;
        float to = startAtLeft ? rightAngle : leftAngle;

        while (true)
        {
            yield return StartCoroutine(RotateFromTo(from, to));
            yield return new WaitForSeconds(holdTime);

            // swap
            float tmp = from;
            from = to;
            to = tmp;
        }
    }

    private IEnumerator RotateFromTo(float aRel, float bRel)
    {
        float startAngle = initialY + aRel;
        float endAngle = initialY + bRel;

        // calcular diferencia angular mínima y duración basada en velocidad
        float angleDiff = Mathf.DeltaAngle(startAngle, endAngle); // puede ser negativo
        float duration = Mathf.Abs(angleDiff) / Mathf.Max(0.0001f, speed);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = duration > 0 ? Mathf.Clamp01(t / duration) : 1f;
            float eval = smooth ? ease.Evaluate(normalized) : normalized;
            float currentY = Mathf.LerpAngle(startAngle, endAngle, eval);
            SetYRotation(currentY);
            yield return null;
        }

        SetYRotation(endAngle);
    }

    private void SetYRotation(float yAbsolute)
    {
        if (useLocalRotation)
        {
            Vector3 e = cameraTransform.localEulerAngles;
            cameraTransform.localEulerAngles = new Vector3(e.x, yAbsolute, e.z);
        }
        else
        {
            Vector3 e = cameraTransform.eulerAngles;
            cameraTransform.eulerAngles = new Vector3(e.x, yAbsolute, e.z);
        }
    }

    // Métodos públicos por si querés arrancar o parar el paneo desde otro script
    public void StopPan()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
    }

    public void StartPan()
    {
        if (loopCoroutine == null)
            loopCoroutine = StartCoroutine(PanLoop());
    }

    // Seguridad: si cambias valores en el inspector mientras editás en modo editor
    void OnValidate()
    {
        if (speed < 0.01f) speed = 0.01f;
        if (ease == null) ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}
