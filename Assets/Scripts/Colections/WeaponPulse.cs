using System.Collections;
using UnityEngine;

public class WeaponPulse : Weapon
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject _myBulletPrebaf;
    [SerializeField] private Transform _instancePoint;
    [SerializeField] private float _timeToShoot = 1f;
    [SerializeField] private float _projectileSpeed = 15f;

    [Header("Particles (optional)")]
    // Prefab que contenga un ParticleSystem (puede ser GameObject o directamente un ParticleSystem)
    [SerializeField] private GameObject _muzzleParticlesPrefab;
    // Si true, parentea las partículas a la bala instanciada (útil para estelas). Si false, las deja en el punto de disparo.
    [SerializeField] private bool _attachParticlesToBullet = false;

    private bool _isReadyToShootAgain = true;

    public void ForceInitialization()
    {
        _isReadyToShootAgain = true;
        StopAllCoroutines(); // Limpiar cualquier corrutina previa
        nameModule = "Blaster";
    }

    public override void Initialized(PlayerMovement player)
    {
        base.Initialized(player);

    }

    public override void PowerElement()
    {
        base.PowerElement();
        _player.CanWeaponChange = true;

        if (!_isReadyToShootAgain) return;

        Shoot();
        _isReadyToShootAgain = false;
        StartCoroutine(TimeToShootAgain());
    }

    private void Shoot()
    {
        // posición y dirección
        Vector3 spawnPos = _instancePoint.position;
        Vector3 forwardDir = _instancePoint.forward;
        forwardDir.y = 0f; // Mantener horizontal (si querés pitch, sacá esta línea)
        forwardDir.Normalize();

        // instanciar bala con rotación mirando hacia la dirección
        GameObject proj = Instantiate(_myBulletPrebaf, spawnPos, Quaternion.LookRotation(forwardDir, Vector3.up));

        // inicializar el script Bullet
        Bullet bulletScript = proj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(forwardDir, _projectileSpeed);
        }
        else
        {
            // fallback si no tiene script Bullet pero sí Rigidbody
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = forwardDir * _projectileSpeed;
        }

        // --- Partículas ---
        // Si asignaste un prefab de partículas, lo instanciamos aquí.
        if (_muzzleParticlesPrefab != null)
        {
            // Instancia en la misma posición y rotación que el instance point
            GameObject psObj = Instantiate(_muzzleParticlesPrefab, spawnPos, Quaternion.LookRotation(forwardDir, Vector3.up));

            // Si querés que las partículas sigan a la bala, las parentamos a la bala instanciada
            if (_attachParticlesToBullet && proj != null)
            {
                psObj.transform.SetParent(proj.transform, true);
            }

            // Intentamos obtener el ParticleSystem para reproducirlo y calcular tiempo de destrucción
            ParticleSystem ps = psObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Asegurarnos de que se reproduzca (en caso de que PlayOnAwake esté desactivado)
                ps.Play();

                // Calcular tiempo seguro para destruir el objeto de partículas:
                var main = ps.main;
                // startLifetime puede ser MinMaxCurve; tomamos constantMax para cubrir el caso máximo
                float lifetime = main.duration + main.startLifetime.constantMax;
                // Agregamos un pequeño margen para que termine de limpiar
                Destroy(psObj, lifetime + 0.25f);
            }
            else
            {
                // Si no hay ParticleSystem (quizá tiene subobjetos), destruimos en 2s por seguridad
                Destroy(psObj, 2f);
            }
        }
    }

    private IEnumerator TimeToShootAgain()
    {
        yield return new WaitForSeconds(_timeToShoot);
        _isReadyToShootAgain = true;
    }

    public override void MyStart()
    {
        _isReadyToShootAgain = true;
    }

    public override void ResetWeaponState()
    {
        base.ResetWeaponState();
    }
}