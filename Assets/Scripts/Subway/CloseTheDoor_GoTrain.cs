using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseTheDoor_GoTrain : MonoBehaviour
{
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] AudioSource _audioSource;

    [Header("Puerta Tren")]
    [SerializeField] GameObject _puertaIZ;
    [SerializeField] GameObject _puertaDER;
    [SerializeField] Transform _puntoA_PuertaIZ;
    [SerializeField] Transform _puntoA_PuertaDER;
    [SerializeField] Transform _puntoB_PuertaDER;
    [SerializeField] AudioClip _soundDoor;
    [SerializeField] private float _speedOpenDoor;

    [SerializeField] GameObject train;
    [SerializeField] Transform _puntoBTrain;
    [SerializeField] float _speedTrain;
    [SerializeField] AudioClip _audioTrains;

    private void OnTriggerEnter(Collider other)
    {
        _playerMovement = other.GetComponent<PlayerMovement>();
        if ( _playerMovement != null)
        {
            _playerMovement._canDo = false;
            _playerMovement.gameObject.transform.SetParent(train.transform);
            //_playerMovement._animatorBasic.animator.SetFloat("Velocity", 0f);
            StartCoroutine(CloseTheDoor());
        }
    }

    private IEnumerator CloseTheDoor()
    {
        Vector3 dir = Vector3.zero;
        _audioSource.PlayOneShot(_soundDoor);
        bool sw_move = true;

        while (sw_move)
        {
            dir = (_puntoA_PuertaDER.position - _puertaDER.transform.position).normalized;
            _puertaDER.transform.position += dir * _speedOpenDoor * Time.deltaTime;
            _puertaIZ.transform.position += -dir * _speedOpenDoor * Time.deltaTime;

            if (Vector3.Distance(_puertaDER.transform.position, _puntoA_PuertaDER.position) <= 0.2f)
            {
                _puertaDER.transform.position = _puntoA_PuertaDER.position;
                _puertaIZ.transform.position = _puntoA_PuertaIZ.position;
                sw_move = false;
            }
            yield return null;
        }
        StartCoroutine(MoveTrain());
    }

    IEnumerator MoveTrain()
    {
        Vector3 dir = Vector3.zero;
        _audioSource.PlayOneShot(_audioTrains);
        bool sw_move = true;

        while (sw_move)
        {
            dir = (_puntoBTrain.position - train.transform.position).normalized;
            train.transform.position += dir * _speedTrain * Time.deltaTime;

            if (Vector3.Distance(_puntoBTrain.position, train.transform.position) <= 0.2f)
            {
                sw_move = false;
            }
            yield return null;
        }
        yield return null;
        PantallaNegra();
    }

    public void PantallaNegra()
    {
        //Aqui va la pantalla negrea
        print("pantalla negra");
    }
}
