using System;
using UnityEngine;

public class EnemyHP : MonoBehaviour 
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private GameObject hpBarUI;
    [SerializeField] private float dieForce;
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
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.material.color = Color.white * intensity;
        }
    }

    public void TakeDamage(float damage , Vector3 direction)
    {
       currentHealth -= damage;
        if(currentHealth <= 0)
        {
            HandleDeath(direction);
        }

        blinkTimer = blinkDuration;
    }
    public float GetHealth()
    {
        return currentHealth;
    }
    private void HandleDeath(Vector3 direction)
    {
        ragdoll.ActivateRagdoll();
        direction.y = 1;
        ragdoll.ApplyForce(direction * dieForce);
        hpBarUI.SetActive(false);
    }
}
