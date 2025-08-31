using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponRadar : Weapon
{
    [SerializeField] private float _radarRadious;
    [SerializeField] private LayerMask _targetLayer; // Capa de objetos a detectar
    [SerializeField] private Material _myLitMaterial;
    [SerializeField] private bool _canRadar = true;

    [SerializeField] private EagleVision _eagleVision;

    public float RadarRadious {  get {  return _radarRadious; } set { _radarRadious = value; } }

    public override void Initialized(PlayerMovement player)
    {
        base.Initialized(player);
        // Obtener referencia al EagleVision del jugador
        _eagleVision = player.GetComponent<EagleVision>();
    }


    public override void PowerElement()
    {

        if (CurrentState != WeaponState.InInventory) return;
        
        if (!_canRadar || _eagleVision == null) return;

        // Activar EagleVision en lugar del efecto de esfera
        _eagleVision.ActivateVision();

        // Detección de objetos (se mantiene igual)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _radarRadious, _targetLayer);
        foreach (Collider collider in hitColliders)
        {
            IPuzzlesElements myPuzzle = collider.GetComponent<IPuzzlesElements>();
            myPuzzle?.ActionPuzzle();
        }

        _canRadar = false;
        StartCoroutine(CanRadarAgain());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radarRadious);
    }

    private IEnumerator CanRadarAgain()
    {
        yield return new WaitForSeconds(3);
        _canRadar = true;
        _player.CanWeaponChange = true;
    }

    public override void ResetWeaponState()
    {
        base.ResetWeaponState();
    }

}
