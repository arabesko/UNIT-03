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
    [SerializeField] bool _canDamage;

    void Start()
    {
        _playerMovement = GameReference.Instance.player.GetComponent<PlayerMovement>();
        _particleSystem.Stop();
        StartGas();
    }

    public void StartGas()
    {
        if (_timeAttack == 0)
        {
            _particleSystem.Play();
            _canDamage = true;
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
        _canDamage = true;
        yield return new WaitForSeconds(_timeAttack);
        _canDamage = false;
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
            if (_canDamage) playerMovement.Damage(_damage);
        }
    }
}
