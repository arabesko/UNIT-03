using UnityEngine;
using Cinemachine;

public class CircleSync : MonoBehaviour
{
    // IDs de propiedades en el shader
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_Size");
    public static int OpacityID = Shader.PropertyToID("_Opacity");  // Nueva propiedad

    [Header("Assign in Inspector")]
    public Material WallMaterial;            // Tu material con shader (_Opacity incluido)
    public CinemachineFreeLook freeLookCam;  // Tu FreeLook virtual
    public LayerMask Mask;                   // Capa “Wall”

    [Header("Circle Settings")]
    [Tooltip("Offset en viewport (0–1) sobre X e Y para centrar finamente")]
    public Vector2 viewportOffset = Vector2.zero;
    [Tooltip("Tamaño del círculo cuando hay bloqueo")]
    [Range(0.1f, 5f)]
    public float circleSize = 1f;
    [Tooltip("Tamaño mínimo (cuando NO hay bloqueo)")]
    [Range(0f, 1f)]
    public float minSize = 0f;

    [Header("Opacity Settings")]
    [Tooltip("Opacidad cuando no hay bloqueo (0 = transparente total)")]
    [Range(0f, 1f)]
    public float minOpacity = 0f;
    [Tooltip("Opacidad cuando HAY bloqueo (1 = totalmente opaco)")]
    [Range(0f, 1f)]
    public float maxOpacity = 1f;

    // Cámara real que pone Cinemachine en escena
    private Camera unityCam;

    void Start()
    {
        // Creamos copia única del material y reasignamos a todos los muros
        WallMaterial = Instantiate(WallMaterial);
        int wallLayer = LayerMask.NameToLayer("Wall");
        foreach (var rend in FindObjectsOfType<MeshRenderer>())
            if (rend.gameObject.layer == wallLayer)
                rend.material = WallMaterial;

        // Cacheamos la Main Camera con el Cinemachine Brain
        unityCam = Camera.main;
        if (unityCam == null)
            Debug.LogError("No se encontró Camera.main. Asegurate de que tu Cinemachine Brain está en la Main Camera.");
    }

    void Update()
    {
        // 1) Raycast desde player hacia cámara real
        Vector3 dir = unityCam.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);
        bool hit = Physics.Raycast(ray, 3000f, Mask);

        // 2) Ajustar tamaño dinámico
        float size = hit ? circleSize : minSize;
        WallMaterial.SetFloat(SizeID, size);

        // 3) Ajustar opacidad dinámica
        float opacity = hit ? maxOpacity : minOpacity;
        WallMaterial.SetFloat(OpacityID, opacity);

        // 4) Convertimos la posición world del player a coords de viewport
        Vector3 view = unityCam.WorldToViewportPoint(transform.position);

        // 5) Aplicamos el offset editable y clamp
        view.x = Mathf.Clamp01(view.x + viewportOffset.x);
        view.y = Mathf.Clamp01(view.y + viewportOffset.y);

        WallMaterial.SetVector(PosID, view);
    }
}