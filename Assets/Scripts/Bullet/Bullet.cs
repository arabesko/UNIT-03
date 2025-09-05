using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _speedRotation = 5f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _targetSearchRange = 10f;
    [SerializeField] private float _lifeTime = 2f;

    private Transform _target;
    private Vector3 _initialDirection = Vector3.forward;
    private bool _initialized = false;

    void Start()
    {
        Destroy(gameObject, _lifeTime);
        // NO sobreescribimos _initialDirection aquí: puede venir desde Initialize.
        FindNearestEnemy();
        // Si no fue inicializado externamente, tomamos transform.forward como fallback
        if (!_initialized)
        {
            _initialDirection = transform.forward;
            _initialDirection.y = 0f;
            if (_initialDirection.sqrMagnitude > 0.0001f)
                _initialDirection.Normalize();
            else
                _initialDirection = Vector3.forward;
            // Orientar visual al inicio
            transform.rotation = Quaternion.LookRotation(_initialDirection, Vector3.up);
        }
    }

    void Update()
    {
        if (_target != null)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;

            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _speedRotation * Time.deltaTime);
        }
        else
        {
            transform.position += _initialDirection * _moveSpeed * Time.deltaTime;

            // Mantener rotación alineada con el movimiento (si querés spinning visual, reemplazá por Rotate)
            Quaternion lookRotation = Quaternion.LookRotation(_initialDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _speedRotation * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamagiable entity = other.GetComponent<IDamagiable>();
        if (entity != null)
        {
            entity.Damage(_damage);
            Destroy(gameObject);
        }
        else
        {
            // opcional: destruir ante paredes u otros impactos
            // Destroy(gameObject);
        }
    }

    private void FindNearestEnemy()
    {
        Scavanger[] enemies = FindObjectsOfType<Scavanger>();
        float minDistance = Mathf.Infinity;
        Scavanger nearestEnemy = null;

        foreach (Scavanger enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance && distance <= _targetSearchRange) // Solo si está en rango
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            _target = nearestEnemy.targetPoint != null ? nearestEnemy.targetPoint : nearestEnemy.transform;
        }
    }

    // --- Método público para inicializar desde WeaponPulse ---
    public void Initialize(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        // Querés disparar estrictamente horizontal: proyectar y = 0
        direction.y = 0f;
        direction.Normalize();

        _initialDirection = direction;
        _moveSpeed = speed;
        _initialized = true;

        // Orientar el transform para que visualmente apunte en esa dirección
        transform.rotation = Quaternion.LookRotation(_initialDirection, Vector3.up);
    }
}