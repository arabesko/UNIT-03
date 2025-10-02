using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scavanger : MonoBehaviour, IDamagiable
{
    [System.Serializable]
    public class MovPoint
    {
        public Transform point;
        [Tooltip("Si true, el enemigo se moverá SOLO en el eje vertical (Y) hacia este punto. Si false, se moverá en el plano horizontal (X,Z). Todos empiezan en horizontal por defecto.")]
        public bool isVertical = false; // por defecto horizontal
    }

    [Header("Enemigo")]
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _damage;

    [Header("Movimiento")]
    [SerializeField] private float _speed;
    [SerializeField] private float _distAttack;
    [SerializeField] private List<MovPoint> _movPoints; // ahora cada punto tiene su bool
    private int _indexMovPoints = 0;
    private Vector3 _dir;
    private Action _currentState;
    private bool _canShase = true;

    [Header("Llegada")]
    [Tooltip("Distancia a la que se considera que llegó al punto (ajustable desde el inspector)")]
    [SerializeField] private float _arrivalTolerance = 0.15f;

    [Header("Rotación")]
    [SerializeField] private float _speedRotation = 5f;
    [Tooltip("Ángulo en grados para corregir la orientación del modelo")]
    [SerializeField] private float offsetY = 90f;
    [Tooltip("Si true, el enemigo también rotará hacia el objetivo cuando el punto sea vertical (útil si querés que gire aunque suba/baje)")]
    [SerializeField] private bool _rotateOnVertical = false;

    // Variables para controlar el giro de 180 grados en vertical
    private bool _isFlipping = false;
    private float _lastVerticalDirection = 0f;

    [Header("Referencias")]
    [SerializeField] private Animator _anim;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _soundDamage;
    [SerializeField] private AudioClip _soundAttack;

    [Header("Externos")]
    [SerializeField] private PlayerMovement _playerScript;
    [SerializeField] private Transform _playerTransform;
    private Vector3 _dirPlayer;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private Transform _origen;

    [Header("Target")]
    public Transform targetPoint;
    public float distBorrar;
    [SerializeField] private bool _canAttack = true;
    [SerializeField] private float _distAttackDamage;

    [Header("Robo de módulos")]
    [SerializeField] private float _dropChance = 0.4f; // 40% de probabilidad
    [SerializeField] private GameObject _modulePrefab; // Prefab para el módulo en el suelo

    [Header("Efectos de Impacto")]
    [SerializeField] private ParticleSystem _hitParticles; // Sistema de partículas
    [SerializeField] private Transform _hitParticlePoint; // Punto donde aparecerán las partículas

    [Header("Stun")]
    [SerializeField] private float _stunDuration = 1.5f;
    private bool _isStunned = false;

    [Header("Debug Visualization")]
    [SerializeField] private Color _visionColor = Color.yellow;
    [SerializeField] private Color _attackColor = Color.red;
    [SerializeField] private Color _detectionOriginColor = Color.blue;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _anim.SetBool("isWalking", true);

        if (_movPoints != null && _movPoints.Count > 0)
        {
            Vector3 initialTarget = GetPointTargetPosition(_indexMovPoints);
            _dir = (initialTarget - transform.position).normalized;
        }

        _currentState = WalkingArround;
    }

    private void Update()
    {
        if (_playerScript._isDeath)
        {
            ResetAnimatorParameters();
            _anim.SetBool("isWalking", true);
            Vector3 t = GetPointTargetPosition(_indexMovPoints);
            _dir = (t - transform.position).normalized;
            _currentState = WalkingArround;
            _canShase = true;
            _canAttack = true;
            return;
        }
        // Si estoy stunneado, no ejecuto ninguna otra lógica
        if (_isStunned)
            return;

        distBorrar = Vector3.Distance(transform.position, _playerScript.transform.position);
        _currentState();

        _dirPlayer = (_playerTransform.position - _origen.position).normalized;

        Debug.DrawLine(_origen.position, _origen.position + _dirPlayer * _distAttack);

        if (Physics.Raycast(transform.position, _dirPlayer, out RaycastHit hit, _distAttack, _playerLayer))
        {
            if (hit.transform.gameObject.layer == _playerTransform.gameObject.layer)
            {
                //El player esta en el rango de ataque
                if (Vector3.Distance(transform.position, _playerScript.transform.position) <= _distAttackDamage)
                {
                    //Lo alcance, debo atacar
                    if (_canAttack)
                    {
                        if (!_playerScript.IsInvisible)
                        {
                            ResetAnimatorParameters();
                            _anim.SetBool("isAttacking", true);
                            _currentState = LookToAttack;
                            _canShase = true;
                            _canAttack = false;
                        }
                        else
                        {
                            ResetAnimatorParameters();
                            _anim.SetBool("isWalking", true);
                            Vector3 t = GetPointTargetPosition(_indexMovPoints);
                            _dir = (t - transform.position).normalized;
                            _currentState = WalkingArround;
                            _canShase = true;
                            _canAttack = true;
                        }
                    }
                }
                else
                {
                    //El player esta en el rango de vision del enemigo y lo estoy persiguiendo
                    if (_canShase && !_playerScript.IsInvisible)
                    {
                        ResetAnimatorParameters();
                        _anim.SetBool("isRunning", true);
                        _currentState = ShasePlayer;
                        _canShase = false;
                        _canAttack = true;
                    }
                    else if (_playerScript.IsInvisible)
                    {
                        ResetAnimatorParameters();
                        _anim.SetBool("isWalking", true);
                        Vector3 t = GetPointTargetPosition(_indexMovPoints);
                        _dir = (t - transform.position).normalized;
                        _currentState = WalkingArround;
                        _canShase = true;
                        _canAttack = true;
                    }
                }
            }
        }
        else
        {
            //Salio del rango de vision
            if (!_canShase)
            {
                ResetAnimatorParameters();
                _anim.SetBool("isWalking", true);
                Vector3 t = GetPointTargetPosition(_indexMovPoints);
                _dir = (t - transform.position).normalized;
                _currentState = WalkingArround;
                _canShase = true;
                _canAttack = true;
            }
            else if (_playerScript._isDeath)
            {
                ResetAnimatorParameters();
                _anim.SetBool("isWalking", true);
                Vector3 t = GetPointTargetPosition(_indexMovPoints);
                _dir = (t - transform.position).normalized;
                _currentState = WalkingArround;
                _canShase = true;
                _canAttack = true;
            }
        }
    }

    private void LookToAttack()
    {
        GirarHacia(_playerTransform.position, 1.5f);
    }

    public void Attack()
    {
        print("Hice daño");
        _audioSource.PlayOneShot(_soundAttack);
        if (Vector3.Distance(transform.position, _playerScript.transform.position) <= _distAttackDamage)
        {
            _playerScript.Damage(_damage);

            // Lógica para robar módulo
            if (UnityEngine.Random.value <= _dropChance && _playerScript.HasModules())
            {
                GameObject stolenModule = _playerScript.DropRandomModule();
                if (stolenModule != null)
                {
                    DropModuleOnFloor(stolenModule);
                }
            }
        }
    }

    // Nuevo método para crear el módulo en el suelo
    private void DropModuleOnFloor(GameObject module)
    {
        Vector3 dropPosition = _playerScript.transform.position +
                          new Vector3(UnityEngine.Random.Range(-1f, 1f),
                                      0.5f,
                                      UnityEngine.Random.Range(-1f, 1f));

        GameObject droppedModule = Instantiate(_modulePrefab, dropPosition, Quaternion.identity);

        ModulePickup pickup = droppedModule.GetComponent<ModulePickup>();
        if (pickup != null)
        {
            pickup.SetModule(module);
        }

        // Forzar desactivación del módulo robado
        Weapon weapon = module.GetComponent<Weapon>();
        if (weapon != null)
        {
            weapon.ForceDisable();
        }
    }
    private void Idle()
    {
        //Lo que sea que haga en idle
    }

    private void WalkingArround()
    {
        if (_isFlipping) return; // No hacer nada mientras está girando

        //Aqui hace la ronda entre los puntos
        if (_movPoints == null || _movPoints.Count == 0) return;

        MovPoint currentMP = _movPoints[_indexMovPoints];
        Vector3 target = GetPointTargetPosition(_indexMovPoints);
        _dir = (target - transform.position).normalized;
        transform.position += _dir * _speed * Time.deltaTime;

        // Decisión de rotación:
        // - Si el punto es horizontal -> rotar siempre (como antes)
        // - Si el punto es vertical -> rotar solo si _rotateOnVertical == true
        if (!currentMP.isVertical || _rotateOnVertical)
        {
            // Si el punto es vertical, al rotar usamos la posición real del punto para orientar correctamente
            Vector3 lookTarget = currentMP.point != null ? currentMP.point.position : target;
            GirarHacia(lookTarget);
        }

        Debug.DrawLine(_origen.position, _origen.position + _dir * 6);

        // Comprueba llegada usando la posición objetivo ya adaptada al eje correspondiente
        if (Vector3.Distance(transform.position, target) < _arrivalTolerance)
        {
            //Llego al punto, preparar siguiente punto
            int nextIndex;
            if (_indexMovPoints == _movPoints.Count - 1)
            {
                nextIndex = 0;
            }
            else
            {
                nextIndex = _indexMovPoints + 1;
            }

            MovPoint nextMP = _movPoints[nextIndex];

            // Verificar si necesita girar 180 grados (solo en movimiento vertical)
            if (currentMP.isVertical && nextMP.isVertical)
            {
                // Comprobar cambio de dirección vertical
                float currentVerticalDir = Mathf.Sign(_dir.y);
                Vector3 nextDir = CalculateDirection(nextMP);
                float nextVerticalDir = Mathf.Sign(nextDir.y);

                if (currentVerticalDir != nextVerticalDir && currentVerticalDir != 0)
                {
                    // Cambió la dirección vertical, girar 180 grados
                    StartCoroutine(Flip180Coroutine(nextIndex));
                    return;
                }
            }

            // Si no necesita girar, continuar normalmente
            _indexMovPoints = nextIndex;

            ResetAnimatorParameters();
            _anim.SetBool("isIdle", true);
            StartCoroutine(TimerIdle());
            _currentState = Idle;
        }
    }

    // NUEVO: Método para calcular dirección
    private Vector3 CalculateDirection(MovPoint movPoint)
    {
        Vector3 direction = movPoint.point.position - transform.position;

        if (movPoint.isVertical)
        {
            return new Vector3(0, direction.y, 0).normalized;
        }
        else
        {
            direction.y = 0;
            return direction.normalized;
        }
    }

    // NUEVO: Corrutina para girar 180 grados
    private IEnumerator Flip180Coroutine(int nextIndex)
    {
        _isFlipping = true;

        // Pequeña pausa antes de girar
        yield return new WaitForSeconds(0.2f);

        float duration = 0.8f;
        float elapsed = 0f;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180f, 0);

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;

        // Continuar con el siguiente punto
        _indexMovPoints = nextIndex;
        _isFlipping = false;

        ResetAnimatorParameters();
        _anim.SetBool("isIdle", true);
        StartCoroutine(TimerIdle());
        _currentState = Idle;
    }

    private void ShasePlayer()
    {
        transform.position += _dirPlayer * _speed * 1.5f * Time.deltaTime;
        GirarHacia(_playerTransform.position, 1.5f);

        if (Vector3.Distance(transform.position, _playerTransform.position) < 1f)
        {
            //Llego al punto, ahora debe atacar

            //ResetAnimatorParameters();
            //_anim.SetBool("isIdle", true);
            //_currentState = Attack;
        }
    }

    private Vector3 GetPointTargetPosition(int index)
    {
        // Retorna la posición objetivo adaptada al eje que corresponde (vertical u horizontal)
        if (_movPoints == null || _movPoints.Count == 0) return transform.position;

        MovPoint mp = _movPoints[index];
        Vector3 p = mp.point != null ? mp.point.position : transform.position;

        if (mp.isVertical)
        {
            // Mantener X,Z actuales, moverse sólo en Y
            return new Vector3(transform.position.x, p.y, transform.position.z);
        }
        else
        {
            // Mantener Y actual, moverse en X,Z
            return new Vector3(p.x, transform.position.y, p.z);
        }
    }

    private void GirarHacia(Vector3 target)
    {
        Vector3 direccion = (target - transform.position);
        direccion.y = 0;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion rotDeseada = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        Quaternion rotCorregida = rotDeseada * Quaternion.Euler(0, offsetY, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotCorregida,
                                              _speedRotation * Time.deltaTime);
    }

    private void GirarHacia(Vector3 target, float moreSpeed)
    {
        Vector3 direccion = (target - transform.position);
        direccion.y = 0;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion rotDeseada = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        Quaternion rotCorregida = rotDeseada * Quaternion.Euler(0, offsetY, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotCorregida,
                                              _speedRotation * moreSpeed * Time.deltaTime);
    }

    private void ResetAnimatorParameters()
    {
        _anim.SetBool("isAttacking", false);
        _anim.SetBool("isRunning", false);
        _anim.SetBool("isWalking", false);
        _anim.SetBool("isIdle", false);
    }

    private IEnumerator TimerIdle()
    {
        yield return new WaitForSeconds(2);
        ResetAnimatorParameters();
        _currentState = WalkingArround;
        _anim.SetBool("isWalking", true);
    }

    public void Health(float health)
    {
    }

    public void Damage(float damage)
    {
        _currentHealth -= damage;
        _audioSource.PlayOneShot(_soundDamage);
        PlayHitParticles();

        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // Arranca el stun
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        _isStunned = true;

        ResetAnimatorParameters();
        _anim.ResetTrigger("Stun");     // Por si quedó activo de antes
        _anim.SetTrigger("Stun");       // Dispara la animación de stun

        Debug.Log("¡Stun trigger activado!");

        yield return new WaitForSeconds(_stunDuration);

        _isStunned = false;

        // Retoma el estado por defecto (puede ser Walking, Idle, etc.)
        ResetAnimatorParameters();
        _anim.SetBool("isWalking", true);
        _currentState = WalkingArround;
    }

    private void PlayHitParticles()
    {
        if (_hitParticles != null)
        {
            var ps = Instantiate(_hitParticles, _hitParticlePoint.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Rango de visión (detección del jugador)
        Gizmos.color = _visionColor;
        Gizmos.DrawWireSphere(transform.position, _distAttack);

        // 2. Rango de ataque (para causar daño)
        Gizmos.color = _attackColor;
        Gizmos.DrawWireSphere(transform.position, _distAttackDamage);

        // 3. Origen de detección (si existe)
        if (_origen != null)
        {
            Gizmos.color = _detectionOriginColor;
            Gizmos.DrawSphere(_origen.position, 0.1f);
            Gizmos.DrawLine(_origen.position, _origen.position + _dirPlayer * _distAttack);
        }

    }

}