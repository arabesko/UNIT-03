using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] float _timeBetweenAttack;
    [SerializeField] float _timeAttack;
    [SerializeField] float _damage;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] PlayerMovement _playerMovement;

    void Start()
    {
        _particleSystem.Stop();
        StartGas();
    }

    public void StartGas()
    {
        if (_timeAttack == 0)
        {
            _particleSystem.Play();
        }
        else
        {
            StartCoroutine(TimeAttack());
        }
    }

    IEnumerator TimeAttack()
    {
        _particleSystem.Play();
        _audioSource.Play();
        yield return new WaitForSeconds(_timeAttack);
        _particleSystem.Stop();
        _audioSource.Stop();
        StartCoroutine(TimeBetweenAttack());
    }

    IEnumerator TimeBetweenAttack()
    {
        yield return new WaitForSeconds(_timeBetweenAttack);
        StartCoroutine(TimeAttack());
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            print("Colision player");
            playerMovement.Damage(_damage);
        }
    }
}
