using System.Collections;
using System.Linq;
using UnityEngine;

public class ScavangerBorrar : Entity
{
    [Header("Búsqueda")]
    public float radioBusqueda;
    public LayerMask capaJugador;

    [Header("Movimiento")]
    public float velocidadMovimiento;
    public float distanciaMaxima;

    [Header("Rotación")]
    public float velocidadRotacion = 5f;
    [Tooltip("Ángulo en grados para corregir la orientación del modelo")]
    public float offsetY = 90f;

    [Header("Referencias")]
    public Rigidbody rb;
    public Animator anim;

    private Transform transformJugador;
    private Vector3 puntoInicial;
    private Coroutine esperaCoroutine;

    public enum EstadosMovimiento { Esperando, Siguiendo, Volviendo, PerdiendoJugador }
    public EstadosMovimiento estadoActual;

    private void Start()
    {
        puntoInicial = transform.position;
        estadoActual = EstadosMovimiento.Esperando;
        ResetAnimatorParameters();
        anim.SetBool("isIdle", true);
    }

    private void Update()
    {
        switch (estadoActual)
        {
            case EstadosMovimiento.Esperando: EstadoEsperando(); break;
            case EstadosMovimiento.Siguiendo: EstadoSiguiendo(); break;
            case EstadosMovimiento.Volviendo: EstadoVolviendo(); break;
            case EstadosMovimiento.PerdiendoJugador: break;
        }
    }

    private void ResetAnimatorParameters()
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);
        anim.SetBool("isIdle", false);
    }

    private void EstadoEsperando()
    {
        ResetAnimatorParameters();
        anim.SetBool("isIdle", true);

        Collider hit = Physics.OverlapSphere(transform.position, radioBusqueda, capaJugador)
                         .FirstOrDefault();
        if (hit != null)
        {
            transformJugador = hit.transform;
            estadoActual = EstadosMovimiento.Siguiendo;
            ResetAnimatorParameters();
            anim.SetBool("isRunning", true);
        }
    }

    private void EstadoSiguiendo()
    {
        velocidadMovimiento = 10f;

        if (!JugadorEnRadioBusqueda())
        {
            IniciarEspera();
            return;
        }

        if (Vector3.Distance(transform.position, puntoInicial) > distanciaMaxima ||
            Vector3.Distance(transform.position, transformJugador.position) > distanciaMaxima)
        {
            IniciarEspera();
            return;
        }

        Vector3 dir = transformJugador.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        GirarHacia(transformJugador.position);
        rb.velocity = new Vector3(dir.x * velocidadMovimiento, rb.velocity.y, dir.z * velocidadMovimiento);
    }

    private void IniciarEspera()
    {
        rb.velocity = Vector3.zero;
        ResetAnimatorParameters();
        anim.SetBool("isIdle", true);

        estadoActual = EstadosMovimiento.PerdiendoJugador;

        if (esperaCoroutine != null) StopCoroutine(esperaCoroutine);
        esperaCoroutine = StartCoroutine(EsperarAntesDeVolver(2f));
    }

    private void EstadoVolviendo()
    {
        velocidadMovimiento = 6.5f;

        ResetAnimatorParameters();
        anim.SetBool("isWalking", true);

        Vector3 dir = puntoInicial - transform.position;
        dir.y = 0;
        dir.Normalize();

        GirarHacia(puntoInicial);
        rb.velocity = new Vector3(dir.x * velocidadMovimiento, rb.velocity.y, dir.z * velocidadMovimiento);

        if (Vector3.Distance(transform.position, puntoInicial) < 0.5f)
        {
            rb.velocity = Vector3.zero;
            ResetAnimatorParameters();
            anim.SetBool("isIdle", true);
            estadoActual = EstadosMovimiento.Esperando;
        }
    }

    private IEnumerator EsperarAntesDeVolver(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (JugadorEnRadioBusqueda() &&
            Vector3.Distance(transform.position, puntoInicial) <= distanciaMaxima &&
            Vector3.Distance(transform.position, transformJugador.position) <= distanciaMaxima)
        {
            ResetAnimatorParameters();
            anim.SetBool("isRunning", true);
            estadoActual = EstadosMovimiento.Siguiendo;
        }
        else
        {
            ResetAnimatorParameters();
            anim.SetBool("isWalking", true);
            estadoActual = EstadosMovimiento.Volviendo;
        }

        esperaCoroutine = null;
    }

    private bool JugadorEnRadioBusqueda()
    {
        if (transformJugador == null) return false;
        return Physics.OverlapSphere(transform.position, radioBusqueda, capaJugador)
                     .Any(c => c.transform == transformJugador);
    }

    private void GirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = (objetivo - transform.position);
        direccion.y = 0;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion rotDeseada = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        Quaternion rotCorregida = rotDeseada * Quaternion.Euler(0, offsetY, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotCorregida,
                                              velocidadRotacion * Time.deltaTime);
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, radioBusqueda);

    //    Vector3 origin = Application.isPlaying ? puntoInicial : transform.position;
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(origin, distanciaMaxima);
    //}
}