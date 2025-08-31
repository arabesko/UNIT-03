using UnityEngine;

public class SphereEffect : MonoBehaviour
{
    [Header("Configuración")]
    public float maxScale = 25f;          // Escala máxima
    public float growthSpeed = 5f;        // Velocidad de crecimiento
    public float effectDuration = 10f;    // Tiempo visible en escala grande
    public float cooldown = 3f;           // Tiempo de espera entre usos

    private Vector3 originalScale;
    private bool isAnimating = false;
    private MeshRenderer sphereRenderer;
    private float lastActivationTime;

    void Start()
    {
        originalScale = transform.localScale;
        sphereRenderer = GetComponent<MeshRenderer>();
        sphereRenderer.enabled = false; // Comienza invisible
        lastActivationTime = -cooldown; // Permite primer uso inmediato
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0) && CanActivate())
        //{
        //    StartEffect();
        //}

        if (isAnimating)
        {
            GrowSphere();
        }
    }

    bool CanActivate()
    {
        // Solo activable si no está animando y ha pasado el cooldown
        return !isAnimating && (Time.time - lastActivationTime >= cooldown);
    }

    public void StartEffect()
    {
        lastActivationTime = Time.time;
        sphereRenderer.enabled = true;
        isAnimating = true;
        transform.localScale = originalScale;
    }

    void GrowSphere()
    {
        // Suavizado exponencial para crecimiento más natural
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            originalScale * maxScale,
            Time.deltaTime * growthSpeed
        );

        // Detección precisa de escala completa
        if (Vector3.Distance(transform.localScale, originalScale * maxScale) < 0.01f)
        {
            Invoke("ResetSphere", effectDuration);
            isAnimating = false; // Detiene la animación pero mantiene el efecto activo
        }
    }

    void ResetSphere()
    {
        sphereRenderer.enabled = false;
        transform.localScale = originalScale;
    }

    //Opcional: Visualizar cooldown en UI
    // void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 10, 200, 20), $"Cooldown: {Mathf.Max(0, cooldown - (Time.time - lastActivationTime)):F1}s");
    //}
}