
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class WeaponInvisible : Weapon
{
    [SerializeField] private GameObject _myBodyInvisible;
    [SerializeField] private Animator _myAnimatorInvisible;
    public override void Initialized(PlayerMovement player)
    {
        base.Initialized(player);
    }

    public override void PowerElement()
    {
        
        if (_player.IsInvisible) return;

        _player.IsInvisible = true;
        StartCoroutine(InvisibleTime());
    }

    public IEnumerator InvisibleTime()
    {
        AcitvateInvisibilityMaterial();
        yield return new WaitForSeconds(5);
        RecoveryMaterial();
    }
    public void RecoveryMaterial()
    {
        MyBodyFBX.SetActive(true);
        _myBodyInvisible.SetActive(false);
        _player._animatorBasic.animator = MyAnimator;
        _player.IsInvisible = false;
        _player.CanWeaponChange = true;
    }

    public void AcitvateInvisibilityMaterial()
    {
        MyBodyFBX.SetActive(false);
        _myBodyInvisible.SetActive(true);
        _player._animatorBasic.animator = _myAnimatorInvisible;
    }

    public override void ResetWeaponState()
    {
        base.ResetWeaponState();
        if (_myBodyInvisible != null) _myBodyInvisible.SetActive(false);
    }
}
