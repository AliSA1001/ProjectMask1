using System;
using UnityEngine;

public class EnemyHP : MonoBehaviour 
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    private RagDoll ragdoll;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    public float blinkIntensity;
    public float blinkDuration;
    private float blinkTimer;
    public ParticleSystem hitParticle;

    private void Awake()
    {
        currentHealth = maxHealth;
       
    }
    private void Start()
    {
        ragdoll = GetComponent<RagDoll>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        var rigedBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var body in rigedBodies)
        {
            body.gameObject.AddComponent<HitBox>();

        }
    }
    private void Update()
    {
       blinkTimer -= Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = lerp * blinkIntensity; //+ 1f; 
        skinnedMeshRenderer.material.color = Color.white * intensity;
    }

    public void TakeDamage(float damage)
    {
       currentHealth -= damage;
        if(currentHealth <= 0)
        {
            HandleDeath();
        }

        blinkTimer = blinkDuration;
    }
    public float GetHealth()
    {
        return currentHealth;
    }
    private void HandleDeath()
    {
        ragdoll.ActivateRagdoll();
    }
}
