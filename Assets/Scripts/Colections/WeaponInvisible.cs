
using System.Collections;
using UnityEngine;

public class WeaponInvisible : Weapon
{
    [SerializeField] private GameObject _myBodyInvisible;
    [SerializeField] private Animator _myAnimatorInvisible;

    // referencias a los materiales de cada parte del cuerpo
    [Header("Dissolve Materials")]
    [SerializeField] private Material headMat;
    [SerializeField] private Material legsMat;
    [SerializeField] private Material leftArmMat;
    [SerializeField] private Material rightArmMat;

    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveSpeed = 2f; // qué tan rápido se disuelve
    [SerializeField] private float invisibleDuration = 5f; // cuánto dura invisible

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

    private IEnumerator InvisibleTime()
    {
        AcitvateInvisibilityMaterial();

        // Aparecer efecto de disolve
        yield return StartCoroutine(DissolveCoroutine(0f, 1f));

        // mantener invisibilidad por X segundos
        yield return new WaitForSeconds(invisibleDuration);

        // Revertir efecto de disolve
        yield return StartCoroutine(DissolveCoroutine(1f, 0f));

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

    /// <summary>
    /// Lerp suave de los 4 materiales al mismo tiempo
    /// </summary>
    private IEnumerator DissolveCoroutine(float start, float end)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * dissolveSpeed;
            float value = Mathf.Lerp(start, end, t);

            headMat.SetFloat("_DisolveHead", value);
            legsMat.SetFloat("_DisolveLegs", value);
            leftArmMat.SetFloat("_DisolveLeft", value);
            rightArmMat.SetFloat("_DisolveRight", value);

            yield return null;
        }
    }
}