using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateNucleo : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _audioClipConnect;
    [SerializeField] Transform _finalPoint;

    [SerializeField] float _speed;
    [SerializeField] float _speedRotation;
    [SerializeField] float offsetY = -90;

    [SerializeField] private ElementPuzzle _myNucleo; //Conexion con el objeto que se levita
    public PlayerMovement _playerMovement;

    [SerializeField] private Train _myTrain;

    [SerializeField] bool _moduleActivate = false;

    private void Start()
    {
        _playerMovement = GameReference.Instance.player.GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<Nucleos>() != null && _moduleActivate == false)
        {
            _myNucleo = other.GetComponent<ElementPuzzle>();
            _playerMovement.NoLevitate();
            _myNucleo.isLevitable = false;
            _myNucleo.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(MoveConnector());
            _moduleActivate = true;
        }
    }

    public IEnumerator MoveConnector()
    {
        bool isFar = true;
        Vector3 dir = Vector3.zero;
        while (isFar)
        {
            dir = (_finalPoint.position - _myNucleo.transform.position).normalized;
            _myNucleo.transform.position += dir * _speed * Time.deltaTime;

            RotateTowards(_finalPoint, _myNucleo.transform);
            if (Vector3.Distance(_finalPoint.transform.position, _myNucleo.transform.position) <= 0.1f)
            {
                isFar = false;
            }
            yield return null;
        }

        _audioSource.PlayOneShot(_audioClipConnect);
        _myTrain.AddNucleo();
        _myNucleo.gameObject.transform.parent = _myTrain._modelo3D.transform; //Deja el nucle destro de los objetos 3D para que se muevan juntos
    }

    private void RotateTowards(Transform target, Transform myTrans)
    {
        Quaternion targetRotation = target.rotation * Quaternion.Euler(0, 0, 0);
        myTrans.rotation = Quaternion.Slerp(myTrans.rotation, targetRotation, _speedRotation * Time.deltaTime);
    }
}
