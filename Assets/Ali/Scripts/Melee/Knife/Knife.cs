using UnityEngine;
using UnityEngine.InputSystem;

public class Knife : MonoBehaviour
{
    [SerializeField] private float timeBetweenAttacks= 0.5f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackDps;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void ResetAttack()
    {
        canAttack = true ;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.started && canAttack)
        {
            animator.SetTrigger("Attacking");
            canAttack = false;
            Invoke("ResetAttack",timeBetweenAttacks);


        }

    }
}
