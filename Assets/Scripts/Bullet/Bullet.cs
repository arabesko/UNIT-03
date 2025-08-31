using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _speedRotation = 5f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _targetSearchRange = 10f;
    private Transform _target;
    private Vector3 _initialDirection;

    void Start()
    {
        Destroy(gameObject, 2f);
        _initialDirection = transform.forward;
        FindNearestEnemy();
    }

    void Update()
    {
        if (_target != null)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _speedRotation * Time.deltaTime);
        }
        else
        {
            transform.position += _initialDirection * _moveSpeed * Time.deltaTime;
            transform.Rotate(0, 0, _speedRotation * Time.deltaTime);
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
}
