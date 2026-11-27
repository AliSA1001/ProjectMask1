using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Knife : MonoBehaviour
{
    [SerializeField] private float timeBetweenAttacks= 0.5f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] public float attackDps;
    [SerializeField] public float currentDurability;

    // HitBox 
    [SerializeField] private BoxCollider hitboxCollider;



    private Animator animator;
    [SerializeField] private Knife_Hitbox hitbox;

    public static Action OnDurabilityLose;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
    }

    private void ResetAttack()
    {
        canAttack = true ;
    }
    private void OnDisable()
    {
        hitboxCollider.enabled = false ;
        animator.SetTrigger("ForceBackToIdle");
    }
    public void DurabilityLose()
    {
        OnDurabilityLose?.Invoke();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.started && canAttack)
        {
            animator.SetTrigger("Attacking");
            canAttack = false;
            Invoke("ResetAttack",timeBetweenAttacks);
            hitbox.ResetList();
            hitboxCollider.enabled = true;


        }

    }
    public void OnEndAttack()
    {
        hitboxCollider.enabled = false;

    }
}
