using Unity.VisualScripting;
using UnityEngine;

public class ComboGarras : StateMachineBehaviour
{
    [Tooltip("Número de golpe: 1, 2 o 3")]
    public int num;
    public int valor = 0;
    private bool hasDashed = false;

    // Cada frame dentro del estado de animación
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Solo durante el primer 10% de la animación, y una sola vez
        if (!hasDashed && Input.GetMouseButtonDown(0))
        {
            hasDashed = true;
            valor = num;
            // Llamamos al dash
            var dash = animator.GetComponent<PlayerDash>()
                       ?? animator.GetComponentInParent<PlayerDash>();
            if (dash != null)
                dash.TriggerDash();
        }

        animator.SetInteger("Control", valor);

    }

    // Al salir del estado, reseteamos la bandera
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasDashed = false;
        valor = 0;
    }
}
