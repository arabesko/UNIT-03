using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Puzzle1 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _myPercent;
    [SerializeField] private int _totalPercent;
    [SerializeField] private Transform _door;
    [SerializeField] private Transform _p1A;
    [SerializeField] private Transform _p1B;
    [SerializeField] private float _speedMoveDoor;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _openDoorClip;

    private void OnTriggerEnter(Collider other)
    {
        PuzzleMother myPuzzle = other.GetComponent<PuzzleMother>();
        if (myPuzzle != null)
        {
            StartCoroutine(GradualIncrement(myPuzzle.MyReturnNumber(), 0.05f, "+"));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PuzzleMother myPuzzle = other.GetComponent<PuzzleMother>();
        if (myPuzzle != null)
        {
            StartCoroutine(GradualIncrement(myPuzzle.MyReturnNumber(), 0.05f, "-"));
        }
    }

    private IEnumerator GradualIncrement(int end, float time, string qqq)
    {
        int i2 = (qqq == "+") ? 1 : -1;

        for (int i = 0; i < end; i++)
        {
            _totalPercent += i2;
            _myPercent.text = _totalPercent.ToString() + "%";
            yield return new WaitForSeconds(time);
        }
        if (_totalPercent >= 100)
        {
            StartCoroutine(OpenTheDoor());

        }
        yield return null;
    }

    private IEnumerator OpenTheDoor()
    {
        // Reproducir sonido una sola vez al iniciar la apertura
        if (_audioSource != null && _openDoorClip != null)
        {
            _audioSource.PlayOneShot(_openDoorClip);
        }

        while (Vector3.Distance(_door.position, _p1B.position) > 0.01f)
        {
            _door.position = Vector3.MoveTowards(_door.position, _p1B.position, _speedMoveDoor * Time.deltaTime);
            yield return null;
        }

        Destroy(this);
    }
}
