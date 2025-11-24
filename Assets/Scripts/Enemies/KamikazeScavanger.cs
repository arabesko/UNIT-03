using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeScavanger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator _anim;
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed;
    [SerializeField] AudioSource _source;
    [SerializeField] AudioClip _clip;

    void Start()
    {
        _anim.SetBool("isRunning", true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            StartCoroutine(AttackKamikaze());
        }
    }

    IEnumerator AttackKamikaze()
    {
        _source.PlayOneShot(_clip);
        Vector3 dir = Vector3.zero;
        bool sw_continue = true;

        while (sw_continue)
        {
            dir = (_target.position - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _target.position) <= 0.2f)
            {
                sw_continue = false;
            }
            yield return null;
        }
        Destroy(gameObject, 1);
    }
}
