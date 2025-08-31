using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public abstract class Entity : MonoBehaviour, IDamagiable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float contactDamage = 10f;

    [Header("Runtime")]
    protected float currentHealth;
    //public Animator animator;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        //animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public virtual void Damage(float amount)
    {
        currentHealth -= amount;
        OnDamageFeedback(amount);
        if (currentHealth <= 0) Die();
    }

    public virtual void Health(float health)
    {
        currentHealth += health;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    protected virtual void OnDamageFeedback(float amount) { }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}