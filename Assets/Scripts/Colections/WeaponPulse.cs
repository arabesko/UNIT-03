using System.Collections;
using UnityEngine;

public class WeaponPulse : Weapon
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject _myBulletPrebaf;
    [SerializeField] private Transform _instancePoint;
    [SerializeField] private float _timeToShoot = 1f;
    [SerializeField] private float _projectileSpeed = 15f;

    private bool _isReadyToShootAgain = true;

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
        forwardDir.y = 0f; //  Mantener horizontal (si querés pitch, sacá esta línea)
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