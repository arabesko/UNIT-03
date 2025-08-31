using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarrasAnimator : MonoBehaviour
{
    [SerializeField] PlayerMovement _playerScript;

    [Header("Animation Settings")]
    [Range(0f, 1f)]
    public float walkAnimValueTransition = 0.1f;

    [HideInInspector] public CharacterController Controller;
    [HideInInspector] public bool EnableMovement = true;
    [HideInInspector] public bool IsDashing = false;
    public bool IsGrounded => Controller.isGrounded;

    private Animator animator;
    private Vector3 velocity;

    void Awake()
    {
        Controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetFloat("Velocity", _playerScript.pct, 0.1f, Time.deltaTime);
    }
}
