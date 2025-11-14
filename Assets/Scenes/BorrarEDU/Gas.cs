using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] float _timeBetweenAttack;
    [SerializeField] float _timeAttack;
    [SerializeField] float _damage;
    void Start()
    {
        if (_timeAttack == 0)
        {
            _particleSystem.Play();
        } else
        {
            StartCoroutine(TimeAttack());
        }
    }

    IEnumerator TimeAttack()
    {
        _particleSystem.Play();
        yield return new WaitForSeconds(_timeAttack);
        _particleSystem.Stop();
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
