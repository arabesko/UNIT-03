using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IDamagiable
{
    [Header("Conecciones")]
    [SerializeField] private PauseMenu _pauseMenu;

    [SerializeField] private UImodules3D moduleUIHighlighter;

    [Header("Player")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    public bool _isDeath;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float rotationSpeed = 10f;
    public bool dontDoNothing = false;

    [Header("Aiming / Bone Aim")]
    [SerializeField] public Transform aimBone;            // Asigná el hueso de la cintura (o el que quieras) desde el inspector
    [SerializeField] private float aimSmoothing = 12f;     // Mayor = más rápido
    [SerializeField] private float minAimDistance = 0.1f;  // Si el mouse está demasiado cerca, no rotar
    public enum RotationAxisOption { WorldUp, BoneLocalUp, BoneLocalRight, BoneLocalForward }
    [SerializeField] private RotationAxisOption rotationAxis = RotationAxisOption.WorldUp;
    [SerializeField] private bool drawAimDebug = true; // para ver rayos en scene view

    private Quaternion aimBoneInitialLocalRot;             // Guardamos rotación local inicial del hueso
    private bool aimBoneInitialized = false;

    [Header("Shooting")]
    [SerializeField] private LayerMask shootableLayers = ~0; // capas que la bala puede alcanzar (todo por defecto)

    [Header("Levitation Sphere Settings")]
    [Tooltip("Prefab de la esfera que se spawnea alrededor del objeto")]
    [SerializeField] private GameObject levitationSpherePrefab;
    private GameObject levitationSphereInstance;
    private Material sphereMaterial;
    private Coroutine dissolveCoroutine;

    [Header("Levitation Settings")]
    public float levitationAmplitude = 0.2f;
    public float levitationFrequency = 1f;
    public Vector2 levitationHeightRange = new Vector2(0.5f, 2f);
    public float maxDistanceFromPlayer = 3f;
    public float levitationRotationSpeed = 30f;
    private float levitationOffset = 0f;
    [SerializeField] private LayerMask _ObstructionLayer;
    public Transform myRayo;

    [Header("Levitation Particles")]
    [SerializeField] private ParticleSystem rayosLevitar;
    [SerializeField] private ParticleSystem arosLevitar;
    private ParticleSystem currentRayosLevitar;
    private ParticleSystem currentArosLevitar;

    [SerializeField] private float _viewRadius;
    [SerializeField] private float _viewAngle;
    public List<GameObject> colectables;
    public LayerMask layerElements;

    [SerializeField] private LayerMask _wallLayer;

    [SerializeField] private GameObject _elementDetected;
    [SerializeField] private GameObject _weaponSelected;

    [SerializeField] private GameObject _elementLevitated;
    public GameObject ElementLevitated { get { return _elementLevitated; } }

    public GameObject ElementDetected
    {
        get { return _elementDetected; }
        set { _elementDetected = value; }
    }

    [SerializeField] private Transform _levitationPoint;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    [Header("Animation Settings")]
    public float pct;
    public float walkAnimValueTransition = 0.1f;

    [SerializeField] public AnimatorBasic _animatorBasic;

    [Header("Interaction Settings")]
    [Tooltip("Radio de interacción para objetos interactuables")]
    public float interactRadius = 5f;
    [Tooltip("Layers que el player puede interactuar (configurable en Inspector)")]
    public LayerMask interactLayers;

    [Header("Inventory")]
    [SerializeField] private GameObject _element0;
    [SerializeField] private Inventory _inventory;

    [HideInInspector] public CharacterController Controller;
    [HideInInspector] public bool EnableMovement = true;
    [HideInInspector] public bool IsDashing = false;

    [SerializeField] private bool _isInvisible = false;

    [SerializeField] private EagleVision _eagleVision;

    [Header("Damage Feedback")]
    [SerializeField] private Light _damageLight;
    [SerializeField] private Material _damageMaterial1; // Primer material
    [SerializeField] private Material _damageMaterial2; // Segundo material (nuevo)
    private Color _originalLightColor;
    private Color _originalMaterialColor1;
    private Color _originalMaterialColor2;
    private bool _colorsSaved = false; // Para asegurar que guardamos los colores solo una vez

    [Header("Wake Animation Settings")]
    [SerializeField] private string wakeAnimationName = "Wake"; // (opcional) trigger name
    [SerializeField] private string wakeStateName = "WakeRevo"; // NOMBRE EXACTO del estado en el Animator
    [SerializeField] private AudioSource wakeAudioSource;
    [SerializeField] private float wakeAnimationDuration = 2f;
    private bool isPlayingWakeAnimation = false;
    private Coroutine wakeCoroutine;

    [Header("Conexion camara")]
    public Transform puntoCamaraFBX;


    public bool IsInvisible
    {
        get { return _isInvisible; }
        set { _isInvisible = value; }
    }
    [Header("BodyRenderer")]
    public List<MeshRenderer> bodyRender;
    [SerializeField] private Transform _projectorPosition;
    [SerializeField] private Transform _module1;

    public bool IsGrounded => Controller.isGrounded;

    private Vector3 velocity;
    private float coyoteCounter, jumpBufferCounter;
    private float currentSpeed;
    private bool isSprinting;

    [SerializeField] private bool _canWeaponChange = true;
    public bool CanWeaponChange
    {
        get { return _canWeaponChange; }
        set { _canWeaponChange = value; }
    }

    void Awake()
    {
        // Manejar DontDestroyOnLoad de forma más robusta
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1)
        {
            // Si ya existe un jugador, destruir este duplicado
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Controller = GetComponent<CharacterController>();

        if (_animatorBasic != null)
            _animatorBasic._playerMovement = this;

        _currentHealth = _maxHealth;

        // Inicializar inventario PRIMERO
        _inventory = new Inventory(8, _element0);

        // Si ya hay objetos iniciales a agregar, llamá AddModules (tu lógica)
        AddModules(_projectorPosition);

        // Ahora sincronizar la UI manager con el inventario
        if (moduleUIHighlighter != null && _inventory != null)
        {
            moduleUIHighlighter.SyncWithInventory(_inventory);
            int sel = _inventory.WeaponSelected;
            moduleUIHighlighter.HighlightObject(sel);
        }

        if (_eagleVision == null)
            _eagleVision = gameObject.AddComponent<EagleVision>();

        SaveOriginalColors();
        if (_damageLight != null)
            _originalLightColor = _damageLight.color;

        if (aimBone != null)
        {
            aimBoneInitialLocalRot = aimBone.localRotation;
            aimBoneInitialized = true;
        }

        // Iniciar animación de wake al comenzar el juego
        //StartWakeAnimation();

        UpdateBlasterLayer();
    }


    public void CollisionWithModule(GameObject myModuleDetected)
    {
        if (CanWeaponChange)
        {
            //Asignamos manualmente a _elementDetected el modulo con el que el player colisiono siempre que se pueda equipar el modulo
            _elementDetected = myModuleDetected;

            //Se activa el modulo colisionado
            AddModules(_module1);
        }
    }



    void LateUpdate()
    {
        

        HandleTimers();
        HandleAimBone();

        _animatorBasic.animator.SetBool("IsGrounded", Controller.isGrounded);
        _animatorBasic.animator.SetBool("IsStunned", false);

        // Solo permitir movimiento si no está en animación de wake
        if (EnableMovement && !isPlayingWakeAnimation)
        {
            HandleMovement();
            HandleGravityAndJump();
        }
        else if (!IsGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            Controller.Move(velocity * Time.deltaTime);
        }

        pct = (currentSpeed > 0f) ? (isSprinting ? 1f : walkAnimValueTransition) : 0f;


        if (dontDoNothing) return;
        // Press
        //if (Input.GetKeyDown(KeyCode.E) && CollectWeapon() && CanWeaponChange)
        //{
        //    //Weapon myWeapon = _elementDetected.GetComponent<Weapon>();
        //    //if (myWeapon != null)
        //        AddModules(_module1);
        //}

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Obtenemos todos los colliders en el rango sin filtrar layers
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius);
            GameObject target = null;
            foreach (var col in hits)
            {
                int layerMaskOfHit = 1 << col.gameObject.layer;
                if ((interactLayers.value & layerMaskOfHit) != 0)
                {
                    target = col.gameObject;
                    break;
                }
            }

            // Si encontramos un objeto interactuable
            if (target != null)
            {
                _animatorBasic.animator.SetTrigger("Press");

                // Evitar spam: una vez interactuado, cambiamos su layer para excluirlo
                
                //ESTO ESTA PROHIBIDO NO HACERLO NUNCA MAS A MENOS QUE NO SE USE MAS EL ELEMENTO
                //target.layer = LayerMask.NameToLayer("Default");

                // Aquí puedes agregar más lógica de interacción aparte de recolectar armas
            }
        }


        // Levitar objetos
        if (Input.GetKeyDown(KeyCode.R) && CollectWeapon() && _elementLevitated == null)
        {
            _elementLevitated = _elementDetected;
            IPuzzlesElements myPuzzle = _elementLevitated.GetComponent<IPuzzlesElements>();

            if (myPuzzle == null)
            {
                _elementLevitated = null;
                return;
            }

            Rigidbody rb = _elementLevitated.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = false;
                rb.freezeRotation = true;
            }

            levitationOffset = 0f;
            myPuzzle.Activate();

            // Instanciar esfera
            if (levitationSpherePrefab != null)
            {
                levitationSphereInstance = Instantiate(
                    levitationSpherePrefab,
                    _elementLevitated.transform.position,
                    Quaternion.identity
                );
                levitationSphereInstance.transform.SetParent(_elementLevitated.transform, false);

                // Obtener material y forzar _Disolve en 1 (disuelto) al inicio
                Renderer rend = levitationSphereInstance.GetComponent<Renderer>();
                if (rend != null)
                {
                    sphereMaterial = rend.material;
                    sphereMaterial.SetFloat("_Disolve", 1f);
                    if (dissolveCoroutine != null) StopCoroutine(dissolveCoroutine);
                    dissolveCoroutine = StartCoroutine(DissolveSphere(1f, 0f, 0.5f, false));
                }
            }

            // AGREGAR SISTEMAS DE PARTÍCULAS - CENTRO DE LA ESFERA
            if (rayosLevitar != null)
            {
                currentRayosLevitar = Instantiate(
                    rayosLevitar,
                    levitationSphereInstance.transform.position,
                    Quaternion.identity,
                    levitationSphereInstance.transform
                );
                currentRayosLevitar.Play();
            }

            if (arosLevitar != null)
            {
                currentArosLevitar = Instantiate(
                    arosLevitar,
                    levitationSphereInstance.transform.position,
                    Quaternion.identity,
                    levitationSphereInstance.transform
                );
                currentArosLevitar.Play();
            }
        }
        // Soltar objeto
        else if (Input.GetKeyDown(KeyCode.R) && _elementLevitated != null)
        {
            NoLevitate();
        }

        // Manejo del objeto levitado
        if (_elementLevitated != null)
        {
            HandleLevitatingObject();
            HandleLevitationSphere();
        }

        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Q)) && _inventory.MyItemsCount() > 0 && CanWeaponChange && !_pauseMenu.isPaused)
        {
            CanWeaponChange = false;
            _weaponSelected.GetComponent<IModules>().PowerElement();
        }

        //Codigo provisorio, esto se debe hacer desde el selector de UI
        if (Input.GetKeyDown(KeyCode.Alpha1) && CanWeaponChange)
        {
            SelectModule(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && CanWeaponChange)
        {
            SelectModule(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && CanWeaponChange)
        {
            SelectModule(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && CanWeaponChange)
        {
            SelectModule(3);
        }
    }


    public Transform target;
    public Transform levitationPointTarget;
    public Transform levitationPointWhenIsCloseWall;
    public LayerMask obstructionToLevitationPoint;
    private void HandleLevitatingObject()
    {
        Vector3 dir = levitationPointTarget.position - myRayo.transform.position;
        float dist = Vector3.Distance(levitationPointTarget.position, myRayo.transform.position);
        //Evaluo si esta mirando hacia una pared mientras levita algo
        if (Physics.Raycast(myRayo.transform.position, dir, out RaycastHit hit, dist, obstructionToLevitationPoint))
        {
            //Hay una pared o algo
            target = levitationPointWhenIsCloseWall;
        }
        else
        {
            //Esta libre
            target = levitationPointTarget;
        }


        levitationOffset += Time.deltaTime * levitationFrequency;
        float yOffset = Mathf.Sin(levitationOffset) * levitationAmplitude;

        Vector3 targetPosition = _levitationPoint.position + new Vector3(0, yOffset, 0);

        float minHeight = transform.position.y + levitationHeightRange.x;
        float maxHeight = transform.position.y + levitationHeightRange.y;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);

        Vector3 horizontalDirection = targetPosition - transform.position;
        horizontalDirection.y = 0;

        if (horizontalDirection.magnitude > maxDistanceFromPlayer)
        {
            horizontalDirection = horizontalDirection.normalized * maxDistanceFromPlayer;
            targetPosition = transform.position + horizontalDirection;
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        }

        _elementLevitated.transform.position = Vector3.Lerp(
            _elementLevitated.transform.position,
            target.position,
            Time.deltaTime * 5f
        );

        if (levitationRotationSpeed > 0)
        {
            _elementLevitated.transform.Rotate(
                Vector3.up,
                levitationRotationSpeed * Time.deltaTime,
                Space.World
            );
        }
    }

    private void HandleLevitationSphere()
    {
        if (levitationSphereInstance == null) return;

        levitationSphereInstance.transform.position = _elementLevitated.transform.position;

        levitationSphereInstance.transform.Rotate(
            Vector3.up,
            levitationRotationSpeed * 0.5f * Time.deltaTime,
            Space.World
        );
    }

    public void NoLevitate()
    {
        if (_elementLevitated == null) return;

        Rigidbody rb = _elementLevitated.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.freezeRotation = false;
        }

        _elementLevitated.GetComponent<IPuzzlesElements>()?.Desactivate();
        _elementLevitated = null;

        // Detener partículas de levitación
        if (currentRayosLevitar != null)
        {
            currentRayosLevitar.Stop();
            Destroy(currentRayosLevitar.gameObject, 2f); // Destruir después de que terminen
        }
        if (currentArosLevitar != null)
        {
            currentArosLevitar.Stop();
            Destroy(currentArosLevitar.gameObject, 2f);
        }

        // Disolver la esfera antes de destruirla
        if (levitationSphereInstance != null && sphereMaterial != null)
        {
            if (dissolveCoroutine != null) StopCoroutine(dissolveCoroutine);
            dissolveCoroutine = StartCoroutine(DissolveSphere(0f, 1f, 0.5f, true));
        }

        // Actualizar estado del cursor
        UpdateCursorState();
    }

    private IEnumerator DissolveSphere(float from, float to, float duration, bool destroyOnEnd)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(from, to, t);
            sphereMaterial.SetFloat("_Disolve", value);
            yield return null;
        }

        if (destroyOnEnd && levitationSphereInstance != null)
        {
            Destroy(levitationSphereInstance);
            levitationSphereInstance = null;
            sphereMaterial = null;
        }
    }

    public void AddModules(Transform _position)
    {
        var myDriver = _elementDetected.GetComponent<IModules>();
        if (myDriver == null) return;

        _weaponSelected = _elementDetected;
        _inventory.AddWeapon(_weaponSelected);

        // Actualizamos UI justo después de agregar
        if (moduleUIHighlighter != null)
        {
            moduleUIHighlighter.SyncWithInventory(_inventory);
            int newIndex = _inventory.WeaponSelected;
            moduleUIHighlighter.HighlightObject(newIndex);
        }

        _weaponSelected.transform.parent = transform;
        _weaponSelected.transform.position = _position.position;
        _weaponSelected.transform.rotation = this.transform.rotation;

        Rigidbody rb = _weaponSelected.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider[] myCol = _weaponSelected.GetComponents<Collider>();
        foreach (Collider item in myCol)
        {
            item.enabled = false;
        }

        SelectModule(_inventory.WeaponSelected);
        myDriver.Initialized(this);

        _elementDetected = null;

        // Forzar actualización del layer Blaster
        UpdateBlasterLayer();
        UpdateCursorState();
    }

    private void SelectModule(int index)
    {
        // Quitamos la verificación de si ya está seleccionado para forzar actualización
        // if (index == _inventory.WeaponSelected) return;

        // Mejor guardia: si index es >= cantidad, no hacer nada
        if (_inventory == null || index >= _inventory.MyItemsCount()) return;

        _weaponSelected = _inventory.SelectWeapon(index);
        _weaponSelected.GetComponent<Weapon>().MyStart();

        // SINCRONIZAR UI por si cambió algo
        if (moduleUIHighlighter != null)
        {
            moduleUIHighlighter.SyncWithInventory(_inventory);
            moduleUIHighlighter.HighlightObject(index);
        }

        // ACTUALIZAR LAYER BLASTER - ESTA ES LA PARTE CLAVE
        UpdateBlasterLayer();

        // Actualizar estado del cursor
        UpdateCursorState();
    }

    private void UpdateBlasterLayer()
    {
        if (_animatorBasic != null && _animatorBasic.animator != null)
        {
            int layerIndex = _animatorBasic.animator.GetLayerIndex("Blaster");

            if (layerIndex != -1)
            {
                bool isBlasterEquipped = (_weaponSelected != null && _weaponSelected.GetComponent<WeaponPulse>() != null);
                _animatorBasic.animator.SetLayerWeight(layerIndex, isBlasterEquipped ? 1f : 0f);

                // Debug para verificar
                Debug.Log($"UpdateBlasterLayer - Weapon: {_weaponSelected?.name}, IsBlaster: {isBlasterEquipped}, LayerWeight: {(isBlasterEquipped ? 1f : 0f)}");
            }
        }
    }


    // método para verificar si tiene módulos
    public bool HasModules()
    {
        return _inventory.MyItemsCount() > 1; // 1 es el módulo base
    }

    public void AddDroppedModule(GameObject module)
    {
        if (module != null && !_inventory.ContainsModule(module))
        {
            Weapon weapon = module.GetComponent<Weapon>();
            if (weapon != null)
            {
                weapon.SetInventoryState();
            }

            module.SetActive(true);
            _inventory.ReAddModule(module);
            if (moduleUIHighlighter != null)
                moduleUIHighlighter.SyncWithInventory(_inventory);

            module.transform.parent = transform;
            Rigidbody rb = module.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Actualizar selección
            _inventory.UpdateWeaponSelection();
        }
    }
    // Método para soltar un módulo aleatorio
    public GameObject DropRandomModule()
    {
        return null;
        if (_inventory.MyItemsCount() <= 1) return null;

        int randomIndex = UnityEngine.Random.Range(1, _inventory.MyItemsCount());
        GameObject module = _inventory.GetModuleAtIndex(randomIndex);

        // Guardar si era el seleccionado antes de remover
        bool wasSelected = (_inventory.WeaponSelected == randomIndex);

        // Remover el módulo
        _inventory.RemoveWeapon(module);

        // Si era el seleccionado, forzar cambio al módulo base
        if (wasSelected)
        {
            // Cambiar al módulo base (índice 0)
            SelectModule(0);

            // Actualizar referencia y animador
            _weaponSelected = _inventory.GetModuleAtIndex(0);
            //_animatorBasic.animator = _inventory.MyCurrentAnimator();
        }

        return module;
    }
    #region Detecciones
    public Vector3 GetVectorFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    private bool CollectWeapon()
    {
        //foreach (var item in colectables)
        //{
        //    if (FieldOfView(item))
        //    {
        //        return true;
        //    }
        //}

        Collider[] hits = Physics.OverlapSphere(transform.position, _viewRadius, layerElements);
        GameObject elementCloser = null;
        float minDist = Mathf.Infinity;
        foreach (var col in hits)
        {
            //Verifico si es un elemento levitable
            ElementPuzzle myPuzz = col.gameObject.GetComponent<ElementPuzzle>();
            if (myPuzz != null)
            {
                if (myPuzz.isLevitable == true)
                {
                    if (Vector3.Distance(transform.position, col.gameObject.transform.position) < minDist)
                    {
                        //Este objeto esta a menor distancia
                        elementCloser = col.gameObject;
                        minDist = Vector3.Distance(transform.position, col.gameObject.transform.position);
                    }
                }
            }
            else
            {
                //Si no es un coleccionable verifico si es un arma
                Weapon myWeapon = col.gameObject.GetComponent<Weapon>();
                if (myWeapon != null)
                {
                    //Es un arma asi que debo decidir 
                    if (Vector3.Distance(transform.position, col.gameObject.transform.position) < minDist)
                    {
                        //Este objeto esta a menor distancia
                        elementCloser = col.gameObject;
                        minDist = Vector3.Distance(transform.position, col.gameObject.transform.position);
                    }
                }
            }
        }

        if (elementCloser != null)
        {
            //Determinar si hay alguna pared que impida 
            //Hay un elemento que es mas cercano que todos los detectados
            if (FreeViewToLevitateObject(elementCloser))
            {
                _elementDetected = elementCloser;
                return true;
            }
        }

        //Si el rayo toca una pared o no hay objetos levitables alrededor
        elementCloser = null;
        return false;
    }

    public bool FreeViewToLevitateObject(GameObject obj)
    {
        Vector3 dir = obj.transform.position - myRayo.transform.position;
        float dist = Vector3.Distance(obj.transform.position, myRayo.transform.position);
        if (Physics.Raycast(myRayo.transform.position, dir, out RaycastHit hit, dist, _ObstructionLayer))
        {
            //Hay una pared en mi camino
            Debug.DrawLine(myRayo.transform.position, obj.transform.position, Color.magenta);
            return false;
        }

        //No hay pared en el camino
        return true;
    }

    public bool FieldOfView(GameObject obj)
    {
        Vector3 dir = obj.transform.position - transform.position;
        if (dir.magnitude < _viewRadius)
        {
            if (Vector3.Angle(transform.forward, dir) < _viewAngle * 0.5f)
            {
                if (!Physics.Raycast(transform.position, dir, out RaycastHit hit, _viewRadius, _wallLayer))
                {
                    Debug.DrawLine(transform.position, obj.transform.position, Color.magenta);
                    _elementDetected = obj;
                    return true;
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.point, Color.magenta);
                    _elementDetected = null;
                }
            }
        }
        return false;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        //Vector3 lineA = GetVectorFromAngle(_viewAngle * 0.5f + transform.eulerAngles.y);
        //Vector3 lineB = GetVectorFromAngle(-_viewAngle * 0.5f + transform.eulerAngles.y);

        //Gizmos.DrawLine(transform.position, transform.position + lineA * _viewRadius);
        //Gizmos.DrawLine(transform.position, transform.position + lineB * _viewRadius);
    }

    #region Movement & Physics
    private void HandleTimers()
    {
        if (Controller.isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = GetCameraRelativeDirection(h, v);

        if (dir.magnitude >= 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);

            isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
            Controller.Move(dir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            currentSpeed = 0f;
            isSprinting = false;
        }
    }

    private Vector3 GetCameraRelativeDirection(float h, float v)
    {
        var cam = Camera.main.transform;
        Vector3 f = cam.forward;
        f.y = 0;
        f.Normalize();

        Vector3 r = cam.right;
        r.y = 0;
        r.Normalize();

        return f * v + r * h;
    }

    private void HandleGravityAndJump()
    {
        // 1. Aplicar gravedad siempre
        velocity.y += gravity * Time.deltaTime;

        // 2. Resetear velocidad vertical al tocar suelo
        if (Controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            coyoteCounter = coyoteTime; // Reiniciar coyote al tocar suelo
        }

        // 3. Comprobar salto DURANTE el coyote time (aunque no estés en el suelo)
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            _animatorBasic.animator?.SetTrigger("Jump");
        }

        // 4. Mover el controlador
        Controller.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region Health System
    public void Health(float health)
    {
        _currentHealth += health;
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    private void SaveOriginalColors() // Guarda los colores que se colocaron desde un principio en el MAT de UI (asi no se quedan siempre en rojo)
    {
        if (_colorsSaved) return;

        if (_damageLight != null)
            _originalLightColor = _damageLight.color;

        if (_damageMaterial1 != null)
            _originalMaterialColor1 = _damageMaterial1.GetColor("_BaseColor");

        if (_damageMaterial2 != null)
            _originalMaterialColor2 = _damageMaterial2.GetColor("_BaseColor");

        _colorsSaved = true;
    }


    private IEnumerator DamageEffect()
    {
        // Asegurarnos de tener los colores originales
        SaveOriginalColors();

        // Cambiar a rojo
        if (_damageLight != null)
            _damageLight.color = Color.red;

        if (_damageMaterial1 != null)
            _damageMaterial1.SetColor("_BaseColor", Color.red);

        if (_damageMaterial2 != null)
            _damageMaterial2.SetColor("_BaseColor", Color.red);

        // Esperar 1 segundo
        yield return new WaitForSeconds(1f);

        // Restaurar colores originales
        RestoreOriginalColors();
    }

    private void RestoreOriginalColors()
    {
        if (_damageLight != null)
            _damageLight.color = _originalLightColor;

        if (_damageMaterial1 != null)
            _damageMaterial1.SetColor("_BaseColor", _originalMaterialColor1);

        if (_damageMaterial2 != null)
            _damageMaterial2.SetColor("_BaseColor", _originalMaterialColor2);
    }



    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // Restaurar colores cuando el objeto se desactiva
        RestoreOriginalColors();
    }

    private void OnDestroy()
    {
        // Restaurar colores cuando el objeto se destruye
        RestoreOriginalColors();
    }
    public void Damage(float damage)
    {
        _currentHealth -= damage;
        // Trigger de stun
        _animatorBasic.animator?.SetTrigger("Stun");
        StartCoroutine(DamageEffect());
        if (_currentHealth <= 0f)
        {
            _isDeath = true;
            StartCoroutine(RespawnWithVolumeFade());
        }
    }
    #endregion

    //**********************************************************************************
    //**********************************************************************************
    //**********************************RESPAWN*****************************************
    //**********************************************************************************
    //**********************************************************************************
    #region RespawnPlayer
    [Header("Respawn Settings")]
    public Transform respawnPoint;         // Punto donde reaparece el jugador
    public float teleportDelay = 1f;       // Tiempo de espera antes de teletransportar al jugador

    [Header("Global Volume Reference")]
    public Volume globalVolume;            // El Global Volume para el efecto
    public float volumeEffectDuration = 2f; // Tiempo total que el efecto permanece activo
    public float fadeDuration = 1f;         // Tiempo de transición para el fade in/out

    private bool isProcessing = false;

    private IEnumerator RespawnWithVolumeFade()
    {
        isProcessing = true;

        // Fade IN del volume (oscurecer)
        if (globalVolume != null)
        {
            globalVolume.enabled = true;
            yield return StartCoroutine(FadeVolume(0f, 1f));
        }

        // Espera antes del teleport
        yield return new WaitForSeconds(teleportDelay);

        // Teletransportar
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = respawnPoint.position;
        _currentHealth = _maxHealth;
        if (cc != null) cc.enabled = true;

        // ---- Aquí lanzamos todo en paralelo ----
        // 1) Start de la animación wake (no esperamos)
        StartCoroutine(PlayWakeAnimationCoroutine());

        // 2) Fade OUT del globalVolume PARA QUE SE VEA LA ANIMACIÓN mientras suena
        if (globalVolume != null)
        {
            // Si querés que la visibilidad vuelva rápido, devolvé este StartCoroutine sin yield
            // pero lo mejor es yield return para que el respawn espere a que visualmente termine el fade.
            yield return StartCoroutine(FadeVolume(1f, 0f));
        }

        // A partir de acá, el volumen está visible; la animación puede seguir en background.
        // Esperamos explícitamente a que termine la animación (si hace falta bloquear más lógica)
        while (isPlayingWakeAnimation)
            yield return null;

        // Seguridad: asegurarnos de apagar el volume si quedó activo
        if (globalVolume != null)
        {
            globalVolume.weight = 0f;
            globalVolume.enabled = false;
        }

        _isDeath = false;
        isProcessing = false;
    }

    private IEnumerator FadeVolume(float from, float to) //As
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            globalVolume.weight = Mathf.Lerp(from, to, t);
            yield return null;
        }

        globalVolume.weight = to;
    }
    #endregion

    #region Wake Animation System
    /// <summary>
    /// Inicia la animación de despertar del robot
    /// </summary>
    public void StartWakeAnimation()
    {
        if (wakeCoroutine != null)
            StopCoroutine(wakeCoroutine);

        wakeCoroutine = StartCoroutine(PlayWakeAnimationCoroutine());
    }

    /// <summary>
    /// Corrutina que controla la animación de despertar
    /// </summary>
    private IEnumerator PlayWakeAnimationCoroutine()
    {
        isPlayingWakeAnimation = true;
        EnableMovement = false;

        // Sonido
        if (wakeAudioSource != null)
        {
            wakeAudioSource.Play();
        }

        // Forzar animación en Animator (más robusto que solo SetTrigger)
        if (_animatorBasic != null && _animatorBasic.animator != null)
        {
            var anim = _animatorBasic.animator;
            if (!anim.enabled) anim.enabled = true;

            // Intentamos Play por nombre de estado (fuerza el estado inmediatamente)
            try
            {
                anim.Play(wakeStateName, 0, 0f); // capa 0, al inicio
                                                 // alternativamente podés usar CrossFade si querés blend:
                                                 // anim.CrossFade(wakeStateName, 0.1f, 0, 0f);
            }
            catch
            {
                // fallback: trigger (por compatibilidad)
                anim.SetTrigger(wakeAnimationName);
            }
        }
        else
        {
            Debug.LogWarning("Animator no asignado en _animatorBasic al intentar PlayWakeAnimation.");
        }

        // Esperamos la duración estimada (puede ser el length real si preferís)
        yield return new WaitForSeconds(wakeAnimationDuration);

        EnableMovement = true;
        isPlayingWakeAnimation = false;
    }


    /// <summary>
    /// Método público para forzar el fin de la animación de wake (por si necesitas cancelarla)
    /// </summary>
    public void EndWakeAnimationEarly()
    {
        if (wakeCoroutine != null)
        {
            StopCoroutine(wakeCoroutine);
            wakeCoroutine = null;
        }

        EnableMovement = true;
        isPlayingWakeAnimation = false;
    }

    /// <summary>
    /// Verifica si está reproduciendo la animación de wake
    /// </summary>
    public bool IsPlayingWakeAnimation()
    {
        return isPlayingWakeAnimation;
    }
    #endregion

    public void UpdateCursorState()
    {
        // Mostrar cursor si está pausado o si tiene WeaponPulse equipado
        if (FindObjectOfType<PauseMenu>().isPaused ||
            (_weaponSelected != null && _weaponSelected.GetComponent<WeaponPulse>() != null))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _animatorBasic.animator.SetBool("IsBlasterEquipped", true);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _animatorBasic.animator.SetBool("IsBlasterEquipped", false);

        }
    }
    private void HandleAimBone()
    {
        if (!aimBoneInitialized || aimBone == null) return;

        // Si no hay arma pulse, volver a initial
        if (_weaponSelected == null || _weaponSelected.GetComponent<WeaponPulse>() == null)
        {
            aimBone.localRotation = Quaternion.Slerp(aimBone.localRotation, aimBoneInitialLocalRot, Time.deltaTime * aimSmoothing);
            return;
        }

        if (!GetMouseWorldPointOnPlane(out Vector3 hitPoint)) return;

        // Dirección deseada (mundo) proyectada en horizontal
        Vector3 desiredDir = hitPoint - aimBone.position;
        desiredDir.y = 0;
        if (desiredDir.sqrMagnitude < 0.0001f) return;
        desiredDir.Normalize();

        // Forward actual del bone (mundo) proyectado horizontal
        Vector3 currentForward = aimBone.forward;
        currentForward.y = 0;
        if (currentForward.sqrMagnitude < 0.0001f) return;
        currentForward.Normalize();

        // Ángulo firmado entre forward actual y direccion deseada (en grados), eje up world
        float signedAngle = Vector3.SignedAngle(currentForward, desiredDir, Vector3.up);

        // Suavizado: hacemos un paso proporcional al smoothing
        float t = Mathf.Clamp01(Time.deltaTime * aimSmoothing);
        float step = signedAngle * t; // cuanto rotar este frame (grados)

        // Eje real en el que vamos a rotar (mundo)
        Vector3 rotationAxisWorld;
        switch (rotationAxis)
        {
            case RotationAxisOption.WorldUp:
                rotationAxisWorld = Vector3.up;
                break;
            case RotationAxisOption.BoneLocalUp:
                rotationAxisWorld = aimBone.TransformDirection(Vector3.up);
                break;
            case RotationAxisOption.BoneLocalRight:
                rotationAxisWorld = aimBone.TransformDirection(Vector3.right);
                break;
            case RotationAxisOption.BoneLocalForward:
                rotationAxisWorld = aimBone.TransformDirection(Vector3.forward);
                break;
            default:
                rotationAxisWorld = Vector3.up;
                break;
        }

        // Rotar el hueso alrededor del eje (en world) pasando por su pivot (posición del bone).
        // Usamos Rotate con Space.World para que el pivote sea el transform mismo y el eje esté en world.
        aimBone.Rotate(rotationAxisWorld, step, Space.World);

        // Debug: dibujar rayos
        if (drawAimDebug)
        {
            Debug.DrawLine(aimBone.position, aimBone.position + currentForward * 1.5f, Color.yellow);
            Debug.DrawLine(aimBone.position, aimBone.position + desiredDir * 2f, Color.cyan);
        }
    }

    // Helper para normalizar un ángulo en grados a rango [-180, 180] para LerpAngle robusto
    private float NormalizeAngle(float angle)
    {
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return angle;
    }

    // Obtiene punto del mouse proyectado en un plano horizontal a la altura del bone
    private bool GetMouseWorldPointOnPlane(out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null) cam = Camera.current;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float planeY = (aimBone != null) ? aimBone.position.y : transform.position.y;
        Plane plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));

        if (plane.Raycast(ray, out float enter))
        {
            hitPoint = ray.GetPoint(enter);
            return true;
        }
        return false;
    }

    public Vector3 GetAimWorldPoint(float defaultDistance = 50f)
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position + transform.forward * defaultDistance;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, defaultDistance, shootableLayers))
        {
            return hit.point;
        }
        // si no choca, devolvemos un punto a distancia fija en esa dirección
        return ray.GetPoint(defaultDistance);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Verificar si esta escena necesita el jugador
        string[] gameScenes = { "SampleScene", "Level2", "Level3" };
        bool shouldBeActive = System.Array.Exists(gameScenes, x => x == scene.name);

        if (!shouldBeActive)
        {
            // En menús, desactivarse
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            // Reinicializar componentes si es necesario
            InitializeInNewScene();
        }
    }

    private void InitializeInNewScene()
    {
        // Reinicializar componentes después de cambiar escena
        if (Controller != null)
        {
            Controller.enabled = true;
        }

        // Resetear velocidad y estado
        velocity = Vector3.zero;
        _elementLevitated = null;

        // Buscar nuevas referencias si es necesario
        if (_pauseMenu == null)
        {
            _pauseMenu = FindObjectOfType<PauseMenu>();
        }
    }

    #region SolucionLevitacionEnParedes

    

    #endregion

}