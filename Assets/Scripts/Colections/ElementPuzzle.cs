using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ElementPuzzle : PuzzleMother
{
    public string fuseID;
    [SerializeField] private int _percent;
    public PlayerMovement _player;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;
    
    //private bool _activateRadar = false;

    public override void Activate()
    {
        base.Activate();
        _activateRotation = true;
    }

    void Start()
    {
        foreach (var part in _MyParts)
        {
            _MyPartsBackup.Add(part.material);
        }
    }

    void Update()
    {
        if (_activateRotation)
        {
            //transform.Rotate(2*_speedRotation * Time.deltaTime, _speedRotation * Time.deltaTime, _speedRotation * Time.deltaTime);
        }
    }

    public override void Desactivate()
    {
        base.Desactivate();
        _activateRotation = false;
    }

    public void OriginalColor()
    {
        for (int i = 0; i < _MyPartsBackup.Count; i++)
        {
            _MyParts[i].material = _MyPartsBackup[i];
        }
    }

    public void DetectionColor()
    {
        foreach(var item in _MyParts)
        {
            item.material = _myMaterialLight;
        }
    }

    public IEnumerator ChangeColorBlink(float time)
    {
        yield return new WaitForSeconds(0.4f);


        if (GameManager.Instance._canBlin)
        {
            _audioSource.PlayOneShot(_audioClip);
            GameManager.Instance._canBlin = false;
            StartCoroutine(CanBlinAgain());
        }

        for (int i = 0; i < 15; i++)
        {
            DetectionColor();
            yield return new WaitForSeconds(time);
            OriginalColor();
            yield return new WaitForSeconds(time);
        }
        //_activateRadar = false;
    }

    public override void ActionPuzzle()
    {
        base.ActionPuzzle();
        //if (_activateRadar) return;
        StartCoroutine(ChangeColorBlink(0.05f));
        //_activateRadar = true;
    }

    public override int MyReturnNumber()
    {
        return _percent;
    }

    private void OnCollisionEnter(Collision collision)
    {
        IWalls myWalls = collision.gameObject.GetComponent<IWalls>();
        if (myWalls != null)
        {
        }
    }

    private IEnumerator CanBlinAgain()
    {
        yield return new WaitForSeconds(3f);
        GameManager.Instance._canBlin = true;
    }
}
