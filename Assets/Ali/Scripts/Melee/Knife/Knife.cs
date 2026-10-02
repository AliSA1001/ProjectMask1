using UnityEngine;
using UnityEngine.InputSystem;

public class Knife : MonoBehaviour
{
    [SerializeField] private float timeBetweenAttacks= 0.5f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackDps;

    // HitBox 
    [SerializeField] private BoxCollider hitboxCollider;


    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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


    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.started && canAttack)
        {
            animator.SetTrigger("Attacking");
            canAttack = false;
            Invoke("ResetAttack",timeBetweenAttacks);
            hitboxCollider.enabled = true;


        }

    }
    public void OnEndAttack()
    {
        hitboxCollider.enabled = false;

    }
}
