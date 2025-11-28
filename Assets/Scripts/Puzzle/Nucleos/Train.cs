using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    [Header("Nucleos")]
    [SerializeField] int _maxNucleos;
    [SerializeField] private int _nNucleos;
    [SerializeField] private bool _isTrainWithNucleos;
    [SerializeField] private bool _isTrainEnergizede;
    [SerializeField] private bool _isRevoInside;

    [SerializeField] private GameObject _redLight;
    [SerializeField] private GameObject _greenLight;

    public GameObject _modelo3D;
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField] private AudioClip _soundMagneto;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private float _speed;

    [Header("Puerta Tren")]
    [SerializeField] GameObject _puertaIZ;
    [SerializeField] GameObject _puertaDER;
    [SerializeField] Transform _puntoA_PuertaDER;
    [SerializeField] Transform _puntoB_PuertaDER;
    [SerializeField] AudioClip _soundDoor;
    [SerializeField] private float _speedOpenDoor;


    public void AddNucleo()
    {
        _nNucleos++;
        if(_nNucleos >= _maxNucleos)
        {
            _isTrainWithNucleos = true;

            if (_isTrainEnergizede && _isTrainWithNucleos)
            {
                ActivateMagnete();
            }
        }
    }

    public void Energize()
    {
        _isTrainEnergizede = true;
        if (_isTrainEnergizede && _isTrainWithNucleos)
        {
            ActivateMagnete();
        }
    }

    public void RevoInsideTrain()
    {
        _isRevoInside = true;
        if (_isTrainEnergizede && _isTrainWithNucleos && _isRevoInside)
        {
            NivelDrivingTrains();
        }
    }

    private void NivelDrivingTrains()
    {
        //Empieza parte 2 del nivel 2
    }

    private void ActivateMagnete()
    {
        _redLight.SetActive(false);
        _greenLight.SetActive(true);
        StartCoroutine(MoveMagneteTrain());
        StartCoroutine(OpenTheDoor());
    }

    private IEnumerator OpenTheDoor()
    {
        Vector3 dir = Vector3.zero;
        _audioSource.PlayOneShot(_soundDoor);
        bool sw_move = true;

        while (sw_move)
        {
            dir = (_puntoB_PuertaDER.position - _puertaDER.transform.position).normalized;
            _puertaDER.transform.position += dir * _speedOpenDoor * Time.deltaTime;
            _puertaIZ.transform.position += -dir * _speedOpenDoor * Time.deltaTime;

            if (Vector3.Distance(_puertaDER.transform.position, _puntoB_PuertaDER.position) <= 0.1f)
            {
                sw_move = false;
            }
            yield return null;
        }
    }

    private IEnumerator MoveMagneteTrain()
    {
        Vector3 dir = Vector3.zero;
        _audioSource.PlayOneShot(_soundMagneto);
        Vector3 target = _pointB.position;
        //bool isFar = true;
        while (true)
        {
            dir = (target - _modelo3D.transform.position).normalized;
            _modelo3D.transform.position += dir * _speed * Time.deltaTime;

            //_modelo3D.transform.position = Vector3.Lerp(_modelo3D.transform.position, target, _speed * Time.deltaTime);


            if (Vector3.Distance(_modelo3D.transform.position, target) <= 0.1f)
            {
                target = (target == _pointB.position)? _pointA.position : _pointB.position;
                //isFar = false;
            }
            yield return null;
        }
    }
}
