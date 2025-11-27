using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyOpenDoor : MonoBehaviour
{
    [SerializeField] GameObject _canvas;
    [SerializeField] GameObject _door;
    [SerializeField] bool _isInArea;
    [SerializeField] Transform _pointB;
    [SerializeField] float _speed;

    [SerializeField] public bool _isCorrectKey;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioPaswordYES;

    [SerializeField] PlayerMovement _playerMovement;
    bool _keyAvalable = true;

    bool _isCursor;

    private void Start()
    {
        _playerMovement = GameReference.Instance.player.gameObject.GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            _isInArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            _isInArea = false;
        }
    }

    private void Update()
    {
        if (_isInArea)
        {
            if (Input.GetKeyDown(KeyCode.E) && _keyAvalable)
            {
                _isCursor = Cursor.visible;
                _playerMovement.CantDo();
                Cursor.visible = true;
                _canvas.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                
            }
        }

        if (_isCorrectKey)
        {
            StartCoroutine(MoveDoor());
            _isCorrectKey = false;
            _keyAvalable = false;
        }
    }

    public void HideKeyBoard()
    {
        _playerMovement.CanDo();
        Cursor.visible = _isCursor;
        _canvas.SetActive(false);
    }

    public IEnumerator MoveDoor()
    {
        _audioSource.PlayOneShot(_audioPaswordYES);
        HideKeyBoard();
        Vector3 dir = (_pointB.position - _door.transform.position).normalized;
        bool goingPoint = true;

        while (goingPoint)
        {
            _door.transform.position += dir * _speed * Time.deltaTime;
            if (Vector3.Distance(_door.transform.position,_pointB.position) <= 0.2f) goingPoint = false;
            yield return null;
        }
    }
}
