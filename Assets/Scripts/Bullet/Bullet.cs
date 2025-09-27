using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _speedRotation = 5f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _targetSearchRange = 10f;
    [SerializeField] private float _lifeTime = 2f;
    [SerializeField] private ParticleSystem _impactParticles; // Sistema de partículas de impacto

    private Transform _target;
    private Vector3 _initialDirection = Vector3.forward;
    private bool _initialized = false;

    void Start()
    {
        Destroy(gameObject, _lifeTime);
        FindNearestEnemy();

        if (!_initialized)
        {
            _initialDirection = transform.forward;
            _initialDirection.y = 0f;
            if (_initialDirection.sqrMagnitude > 0.0001f)
                _initialDirection.Normalize();
            else
                _initialDirection = Vector3.forward;
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
            Quaternion lookRotation = Quaternion.LookRotation(_initialDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _speedRotation * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Generar partículas de impacto
        if (_impactParticles != null)
        {
            Instantiate(_impactParticles, transform.position, Quaternion.identity);
        }

        // Dañar enemigos si es posible
        IDamagiable entity = other.GetComponent<IDamagiable>();
        if (entity != null)
        {
            entity.Damage(_damage);
        }

        // Destruir la bala siempre que colisione con cualquier cosa
        Destroy(gameObject);
    }

    private void FindNearestEnemy()
    {
        Scavanger[] enemies = FindObjectsOfType<Scavanger>();
        float minDistance = Mathf.Infinity;
        Scavanger nearestEnemy = null;

        foreach (Scavanger enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance && distance <= _targetSearchRange)
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

    public void Initialize(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        direction.y = 0f;
        direction.Normalize();

        _initialDirection = direction;
        _moveSpeed = speed;
        _initialized = true;

        transform.rotation = Quaternion.LookRotation(_initialDirection, Vector3.up);
    }
}