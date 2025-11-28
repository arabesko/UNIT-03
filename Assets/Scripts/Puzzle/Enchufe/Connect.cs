using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connect : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _audioClipConnect;
    [SerializeField] Transform _finalPoint;

    [SerializeField] float _speed;
    [SerializeField] float _speedRotation;
    [SerializeField] float offsetY = -90;

    [SerializeField] private Desconect _enchufe; //Conexion con el objeto que se levita
    public PlayerMovement _playerMovement;

    [SerializeField] private Train _myTrain;
    [SerializeField] bool _moduleActivate = false;

    private void Start()
    {
        _playerMovement = GameReference.Instance.player.GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponent<Desconect>() != null && _moduleActivate == false)
        {
            _enchufe = other.GetComponent<Desconect>();
            _playerMovement.NoLevitate();
            _enchufe.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(MoveConnector());
            _myTrain.Energize();
            _moduleActivate = false;
        }
    }

    public IEnumerator MoveConnector()
    {
        _enchufe.myElementPuzzle.isLevitable = false;
        bool isFar = true;
        Vector3 dir = Vector3.zero;
        while (isFar)
        {
            dir = (_finalPoint.position - _enchufe.transform.position).normalized;
            _enchufe.transform.position += dir * _speed * Time.deltaTime;

            RotateTowards(_finalPoint, _enchufe.transform);
            if (Vector3.Distance(_finalPoint.transform.position, _enchufe.transform.position) <= 0.1f)
            {
                isFar = false;
            }

            yield return null;
        }
        _audioSource.PlayOneShot(_audioClipConnect);
        
    }

    private void RotateTowards(Transform target, Transform myTrans)
    {
        Quaternion targetRotation = target.rotation * Quaternion.Euler(0, 0, 0);
        myTrans.rotation = Quaternion.Slerp(myTrans.rotation, targetRotation, _speedRotation * Time.deltaTime);
    }
}
