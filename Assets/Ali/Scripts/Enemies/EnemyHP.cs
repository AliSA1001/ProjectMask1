using System;
using UnityEngine;

public class EnemyHP : MonoBehaviour 
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    private RagDoll ragdoll;


    private void Awake()
    {
        currentHealth = maxHealth;
       
    }
    private void Start()
    {
        ragdoll = GetComponent<RagDoll>();

        var rigedBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var body in rigedBodies)
        {
            body.gameObject.AddComponent<HitBox>();

        }
    }
    private void Update()
    {
       
    }

    public void TakeDamage(float damage)
    {
       currentHealth -= damage;
        if(currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        ragdoll.ActivateRagdoll();
    }
}
