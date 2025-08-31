using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorBasic : MonoBehaviour
{
    public PlayerMovement _playerMovement;

    [Header("Animation Settings")]
    [Range(0f, 1f)]
    public float walkAnimValueTransition = 0.1f;

    [HideInInspector] public bool EnableMovement = true;
    [HideInInspector] public bool IsDashing = false;

    [SerializeField] public Animator animator;

    void Update()
    {
        //print(_playerMovement.pct);
        UpdateAnimation();
    }

    void ChangeAnimator(Animator myAnimator)
    {
        animator = myAnimator;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetFloat("Velocity", _playerMovement.pct, 0.1f, Time.deltaTime);
    }
}
