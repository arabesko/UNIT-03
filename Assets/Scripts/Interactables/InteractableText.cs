//using TMPro;
//using UnityEngine;

//public class InteractableText : MonoBehaviour
//{
//    [Header("Prefab de texto (TextMeshPro 3D)")]
//    [Tooltip("Debe ser un GameObject con un componente TextMeshPro (no UGUI).")]
//    [SerializeField] private TextMeshPro textPrefab;

//    [Header("Mensaje a mostrar")]
//    [TextArea]
//    [SerializeField] private string message = "¡Hola!";

//    [Header("Distancia de activación")]
//    [SerializeField] private float showDistance = 5f;

//    [Header("Offset local sobre el objeto")]
//    [SerializeField] private Vector3 localOffset = new Vector3(0, 2f, 0);

//    [Header("Animación de levitación")]
//    [SerializeField] private float floatAmplitude = 0.2f;   // Qué tanto sube y baja
//    [SerializeField] private float floatFrequency = 2f;     // Velocidad de oscilación

//    [Header("Control externo del texto")]
//    public bool interactableEnabled = true;

//    private Transform _player;
//    [SerializeField] private PlayerMovement _playerScript;
//    private TextMeshPro _instanceTMP;
//    private float _baseY;

//    public bool _isUIActivate = true;

//    private void Start()
//    {
//        // Buscar al jugador por tag
//        var go = GameObject.FindGameObjectWithTag("Player");
//        if (go != null)
//        {
//            _player = go.transform;
//        }
//        else
//        {
//            Debug.LogWarning("No se encontró ningún GameObject con tag 'Player'.");
//        }

//        // Instanciar el texto como objeto independiente
//        if (textPrefab != null)
//        {
//            var goText = Instantiate(textPrefab.gameObject);
//            _instanceTMP = goText.GetComponent<TextMeshPro>();
//            _instanceTMP.gameObject.SetActive(false);

//            // Mantener la escala original del prefab
//            goText.transform.localScale = textPrefab.transform.localScale;
//        }
//        else
//        {
//            //Debug.LogError("Asigna un TextMeshPro 3D prefab en el Inspector.");
//        }

//        // Valor base para el movimiento
//        _baseY = localOffset.y;
//    }

//    private void LateUpdate()
//    {
//        if (_player == null || _instanceTMP == null || !interactableEnabled) return;

//        float dist = Vector3.Distance(_player.position, transform.position);
//        bool shouldShow = dist <= showDistance;

//        // Verificar si el objeto está en el inventario (es hijo del jugador)
//        bool isInInventory = transform.IsChildOf(_player);

//        // Si el objeto está siendo levitado o está en inventario, ocultar el texto
//        if ((_instanceTMP.gameObject.activeSelf && _playerScript.ElementLevitated != null) || isInInventory)
//        {
//            HideUILetter();
//        }

//        // Salir temprano si el objeto está levitado o en inventario
//        if (_playerScript.ElementLevitated != null || isInInventory) return;

//        // Manejar visibilidad del texto
//        if (shouldShow && !_instanceTMP.gameObject.activeSelf && _isUIActivate)
//        {
//            _instanceTMP.text = message;
//            _instanceTMP.gameObject.SetActive(true);
//        }
//        else if (!shouldShow && _instanceTMP.gameObject.activeSelf)
//        {
//            _instanceTMP.gameObject.SetActive(false);
//        }

//        // Animación y rotación solo si está activo y se debe mostrar
//        if (shouldShow && _instanceTMP.gameObject.activeSelf)
//        {
//            float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
//            Vector3 animatedOffset = new Vector3(localOffset.x, _baseY + floatOffset, localOffset.z);

//            // Mantener la escala original
//            _instanceTMP.transform.localScale = textPrefab.transform.localScale;

//            _instanceTMP.transform.position = transform.position + animatedOffset;

//            Vector3 dir = _instanceTMP.transform.position - Camera.main.transform.position;
//            _instanceTMP.transform.rotation = Quaternion.LookRotation(dir);
//        }
//    }

//    public void HideUILetter()
//    {
//        _instanceTMP.gameObject.SetActive(false);
//    }
//}