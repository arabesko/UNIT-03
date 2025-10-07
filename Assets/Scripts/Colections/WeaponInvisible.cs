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
    [SerializeField] private float SpeedFBX = 0f; // Nueva variable
    [SerializeField] private float NoiseFBX = 0f; // Nueva variable

    public override void Initialized(PlayerMovement player)
    {
        base.Initialized(player);
        nameModule = "Invisible";
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
        _player.IsInvisible = false;
        _player.CanWeaponChange = true;
    }

    public void AcitvateInvisibilityMaterial()
    {
        //MyBodyFBX.SetActive(false);
        //_myBodyInvisible.SetActive(true);
        //_player._animatorBasic.animator = _myAnimatorInvisible;
    }

    public override void ResetWeaponState()
    {
        base.ResetWeaponState();
        //if (_myBodyInvisible != null) _myBodyInvisible.SetActive(false);
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

            // Propiedades de dissolve originales
            headMat.SetFloat("_Disolve", value);
            legsMat.SetFloat("_Disolve", value);
            leftArmMat.SetFloat("_Disolve", value);
            rightArmMat.SetFloat("_Disolve", value);

            // Nuevas propiedades FBX (solo se activan durante la transición)
            float fbxValue = Mathf.Lerp(0f, 0.1f, t);
            headMat.SetFloat("_SpeedFBX", fbxValue);
            headMat.SetFloat("_NoiseFBX", fbxValue);
            legsMat.SetFloat("_SpeedFBX", fbxValue);
            legsMat.SetFloat("_NoiseFBX", fbxValue);
            leftArmMat.SetFloat("_SpeedFBX", fbxValue);
            leftArmMat.SetFloat("_NoiseFBX", fbxValue);
            rightArmMat.SetFloat("_SpeedFBX", fbxValue);
            rightArmMat.SetFloat("_NoiseFBX", fbxValue);

            yield return null;
        }

        // Asegurar valores finales después del bucle
        float finalFBXValue = end == 1f ? 0.1f : 0f;
        headMat.SetFloat("_SpeedFBX", finalFBXValue);
        headMat.SetFloat("_NoiseFBX", finalFBXValue);
        legsMat.SetFloat("_SpeedFBX", finalFBXValue);
        legsMat.SetFloat("_NoiseFBX", finalFBXValue);
        leftArmMat.SetFloat("_SpeedFBX", finalFBXValue);
        leftArmMat.SetFloat("_NoiseFBX", finalFBXValue);
        rightArmMat.SetFloat("_SpeedFBX", finalFBXValue);
        rightArmMat.SetFloat("_NoiseFBX", finalFBXValue);
    }
}