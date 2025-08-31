using UnityEngine;
using System.Collections;

public class BaseScavanger : Entity
{
    //public Transform[] patrolPoints;
    //public float patrolSpeed = 2f;
    //public float chaseSpeed = 4f;
    //public float chaseDistance = 5f;
    //public float waitTime = 2f;
    //public float stunDuration = 1.5f;

    //int currentPoint = 0;
    //Transform player;
    //bool isWaiting;

    //void Awake()
    //{
    //    base.Awake();
    //    player = GameObject.FindGameObjectWithTag("Player").transform;
    //}

    //void Start() => StartPatrol();

    //void Update()
    //{
    //    if (animator.GetBool("isStunned")) return;

    //    float dist = Vector3.Distance(transform.position, player.position);
    //    if (dist <= chaseDistance)
    //        StartChase();
    //    else if (animator.GetBool("isChasing"))
    //        ReturnToPatrol();
    //}

    //void StartPatrol()
    //{
    //    StopAllCoroutines();
    //    animator.SetBool("isStunned", false);
    //    animator.SetBool("isChasing", false);
    //    animator.SetBool("isIdle", false);
    //    animator.SetBool("isPatrolling", true);
    //    MoveToNextPoint();
    //}

    //void MoveToNextPoint()
    //{
    //    if (patrolPoints.Length == 0) return;
    //    Vector3 target = patrolPoints[currentPoint].position;
    //    StartCoroutine(Move(target, patrolSpeed, () =>
    //    {
    //        StartCoroutine(Wait(waitTime, StartPatrol));
    //        animator.SetBool("isIdle", true);
    //        animator.SetBool("isPatrolling", false);
    //    }));
    //    currentPoint = (currentPoint + 1) % patrolPoints.Length;
    //}

    //void StartChase()
    //{
    //    StopAllCoroutines();
    //    animator.SetBool("isIdle", false);
    //    animator.SetBool("isPatrolling", false);
    //    animator.SetBool("isChasing", true);
    //    StartCoroutine(Move(player.position, chaseSpeed, () => { }));
    //}

    //void ReturnToPatrol()
    //{
    //    animator.SetBool("isChasing", false);
    //    StartCoroutine(Wait(waitTime, StartPatrol));
    //    animator.SetBool("isIdle", true);
    //}

    //IEnumerator StunRoutine()
    //{
    //    StopAllCoroutines();
    //    animator.SetBool("isStunned", true);
    //    yield return new WaitForSeconds(stunDuration);
    //    animator.SetBool("isStunned", false);
    //    ReturnToPatrol();
    //}

    //protected override void OnDamageFeedback(float amount)
    //{
    //    StartCoroutine(StunRoutine());
    //}

    //IEnumerator Move(Vector3 dest, float speed, System.Action onArrive)
    //{
    //    while (Vector3.Distance(transform.position, dest) > 0.1f)
    //    {
    //        Vector3 dir = (dest - transform.position).normalized;
    //        transform.position += dir * speed * Time.deltaTime;
    //        transform.rotation = Quaternion.Slerp(
    //            transform.rotation,
    //            Quaternion.LookRotation(dir),
    //            10f * Time.deltaTime);
    //        yield return null;
    //    }
    //    onArrive?.Invoke();
    //}

    //IEnumerator Wait(float seconds, System.Action onComplete)
    //{
    //    yield return new WaitForSeconds(seconds);
    //    onComplete?.Invoke();
    //}
}