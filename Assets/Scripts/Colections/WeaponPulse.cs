using System.Collections;
using UnityEngine;

public class WeaponPulse : Weapon
{
    [SerializeField] public GameObject _myBulletPrebaf;
    [SerializeField] public Transform _instancePoint;
    [SerializeField] public float _timeToShoot = 1;
    [SerializeField] public bool _isReadyToShootAgain = true;

    public override void Initialized(PlayerMovement player)
    {
        base.Initialized(player);
    }

    public override void PowerElement()
    {
        
        base.PowerElement();
        _player.CanWeaponChange = true;
        if (!_isReadyToShootAgain) return;
        Instantiate(_myBulletPrebaf, _instancePoint.position, transform.rotation);
        _isReadyToShootAgain = false;
        StartCoroutine(TimeToShootAgain());
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