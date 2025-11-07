using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    [SerializeField] int _maxNucleos;
    [SerializeField] private int _nNucleos;
    [SerializeField] private bool _isTriainWithNucleos;
    [SerializeField] private bool _isTriainEnergizede;
    [SerializeField] private bool _isRevoInside;


    public void AddNucleo()
    {
        _nNucleos++;
        if(_nNucleos >= _maxNucleos)
        {
            _isTriainWithNucleos = true;
        }
    }

    public void Energize()
    {
        _isTriainEnergizede = true;
    }

    public void RevoInsideTrain()
    {
        _isRevoInside = true;
        if (_isTriainEnergizede && _isTriainWithNucleos && _isRevoInside)
        {
            NivelDrivingTrains();
        }
    }

    private void NivelDrivingTrains()
    {
        //Empieza parte 2 del nivel 2
    }

}
