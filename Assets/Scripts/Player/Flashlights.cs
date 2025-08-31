using UnityEngine;

public class Flashlights : MonoBehaviour
{
    [Header("Conecciones")]
    [SerializeField] private PauseMenu _pauseMenu; 

    [Header("Lights (Linternas)")]
    [SerializeField] private Light linternaIzquierda;
    [SerializeField] private Light linternaDerecha;

    [Header("Eyes (Ojos)")]
    [SerializeField] private GameObject OjoIZQ;
    [SerializeField] private GameObject OjoDER;

    [Header("Head Bones")]
    [SerializeField] private Transform[] targetHuesos; // Array de huesos de cabeza

    [Header("Light Position Adjustments")]
    [SerializeField] private float alturaLinternaIzquierda = 0.2f;
    [SerializeField] private float alturaLinternaDerecha = 0.2f;
    [SerializeField] private float desplazamientoHorizontalIzquierda = -0.1f;
    [SerializeField] private float desplazamientoHorizontalDerecha = 0.1f;

    [Header("Eye Position Adjustments (local space respecto al hueso)")]
    [SerializeField] private float alturaOjoIzquierda = 0.0f;         // offset en Y (arriba/abajo)
    [SerializeField] private float alturaOjoDerecha = 0.0f;
    [SerializeField] private float desplazamientoHorizontalOjoIzquierda = -0.05f; // offset en X (izq/der)
    [SerializeField] private float desplazamientoHorizontalOjoDerecha = 0.05f;
    [SerializeField] private float profundidadOjoIzquierda = 0.05f;  // offset en Z (adelante/atrás)
    [SerializeField] private float profundidadOjoDerecha = 0.05f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoEncender;
    [SerializeField] private AudioClip sonidoApagar;

    private AudioSource audioSource;
    private bool lucesEncendidas;
    private Transform activeBone; // Hueso activo actual

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lucesEncendidas = (linternaIzquierda != null) && linternaIzquierda.enabled;
        FindActiveBone(); // Buscar hueso activo inicial

        // Asegurar estado inicial de los ojos (si los tenemos)
        if (OjoIZQ != null) OjoIZQ.SetActive(lucesEncendidas);
        if (OjoDER != null) OjoDER.SetActive(lucesEncendidas);
    }

    void LateUpdate()
    {
        // Actualizamos siempre que las luces estén encendidas (igual que tus linternas)
        if (lucesEncendidas)
        {
            // Actualizar si el hueso activo cambió
            if (activeBone == null || !activeBone.gameObject.activeInHierarchy)
            {
                FindActiveBone();
            }

            if (activeBone != null)
            {
                UpdateLightTransforms(); // mantiene la lógica de linternas como tenías
                UpdateEyeTransforms();   // actualiza las esferas/ojos
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !_pauseMenu.isPaused)
        {
            ToggleLuces();
        }
    }

    // Busca el primer hueso activo en el array
    private void FindActiveBone()
    {
        foreach (Transform bone in targetHuesos)
        {
            if (bone != null && bone.gameObject.activeInHierarchy)
            {
                activeBone = bone;
                return;
            }
        }
        activeBone = null;
    }

    private void UpdateLightTransforms()
    {
        if (activeBone == null) return;

        // Mantenemos posición original del hueso
        Vector3 posicionHueso = activeBone.position;

        // Aplicar ajustes de posición a cada linterna (sin cambiar tu lógica)
        if (linternaIzquierda != null)
        {
            linternaIzquierda.transform.position = new Vector3(
                posicionHueso.x + desplazamientoHorizontalIzquierda,
                posicionHueso.y + alturaLinternaIzquierda,
                posicionHueso.z
            );
            linternaIzquierda.transform.rotation = activeBone.rotation;
        }

        if (linternaDerecha != null)
        {
            linternaDerecha.transform.position = new Vector3(
                posicionHueso.x + desplazamientoHorizontalDerecha,
                posicionHueso.y + alturaLinternaDerecha,
                posicionHueso.z
            );
            linternaDerecha.transform.rotation = activeBone.rotation;
        }
    }

    private void UpdateEyeTransforms()
    {
        if (activeBone == null) return;

        // Usamos offsets en el espacio local del hueso para que roten/muevan correctamente con la cabeza.
        // Vector3(localX, localY, localZ) -> TransformPoint convierte a world space teniendo en cuenta la rotación.
        if (OjoIZQ != null)
        {
            Vector3 localOffsetIzq = new Vector3(
                desplazamientoHorizontalOjoIzquierda,
                alturaOjoIzquierda,
                profundidadOjoIzquierda
            );
            OjoIZQ.transform.position = activeBone.TransformPoint(localOffsetIzq);
            OjoIZQ.transform.rotation = activeBone.rotation;
        }

        if (OjoDER != null)
        {
            Vector3 localOffsetDer = new Vector3(
                desplazamientoHorizontalOjoDerecha,
                alturaOjoDerecha,
                profundidadOjoDerecha
            );
            OjoDER.transform.position = activeBone.TransformPoint(localOffsetDer);
            OjoDER.transform.rotation = activeBone.rotation;
        }
    }

    private void ToggleLuces()
    {
        lucesEncendidas = !lucesEncendidas;

        if (linternaIzquierda != null) linternaIzquierda.enabled = lucesEncendidas;
        if (linternaDerecha != null) linternaDerecha.enabled = lucesEncendidas;

        // Activar/desactivar los GameObjects de los ojos (así se comportan igual que las linternas).
        if (OjoIZQ != null) OjoIZQ.SetActive(lucesEncendidas);
        if (OjoDER != null) OjoDER.SetActive(lucesEncendidas);

        if (audioSource != null)
            audioSource.PlayOneShot(lucesEncendidas ? sonidoEncender : sonidoApagar);

        // Buscar hueso activo al encender
        if (lucesEncendidas) FindActiveBone();
    }

    // Método para forzar actualización de huesos
    public void RefreshBones()
    {
        FindActiveBone();
        if (lucesEncendidas)
        {
            UpdateLightTransforms();
            UpdateEyeTransforms();
        }
    }

    // Métodos para ajustar posición en tiempo real (linternas)
    public void SetDesplazamientoHorizontal(float izquierda, float derecha)
    {
        desplazamientoHorizontalIzquierda = izquierda;
        desplazamientoHorizontalDerecha = derecha;
        if (lucesEncendidas) UpdateLightTransforms();
    }

    public void SetAlturaLinternas(float altura)
    {
        alturaLinternaIzquierda = altura;
        alturaLinternaDerecha = altura;
        if (lucesEncendidas) UpdateLightTransforms();
    }

    // Métodos para ajustar posición en tiempo real (ojos)
    public void SetDesplazamientoHorizontalOjos(float izquierda, float derecha)
    {
        desplazamientoHorizontalOjoIzquierda = izquierda;
        desplazamientoHorizontalOjoDerecha = derecha;
        if (lucesEncendidas) UpdateEyeTransforms();
    }

    public void SetAlturaOjos(float alturaIzq, float alturaDer)
    {
        alturaOjoIzquierda = alturaIzq;
        alturaOjoDerecha = alturaDer;
        if (lucesEncendidas) UpdateEyeTransforms();
    }

    public void SetProfundidadOjos(float profundidadIzq, float profundidadDer)
    {
        profundidadOjoIzquierda = profundidadIzq;
        profundidadOjoDerecha = profundidadDer;
        if (lucesEncendidas) UpdateEyeTransforms();
    }
}