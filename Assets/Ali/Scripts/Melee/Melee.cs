using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Melee : MonoBehaviour
{
  

    [SerializeField] private float windUpTime = 0.34f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float damage = 10;

    private Animator animator;
    private bool canAttack = true;
    


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed && canAttack)
        {
            StartCoroutine(Attack());
        }
    }
    IEnumerator Attack()
    {
        canAttack = false;
        animator.Play("Knife_Attack");

        yield return new WaitForSeconds(windUpTime);

        Collider[] hits = Physics.OverlapSphere(attackPoint.position,attackRange, hitLayer);

        List<GameObject> ListOfTargetHits = new List<GameObject>();

        foreach (Collider hit in hits)
        {
          GameObject  hitTarget = hit.transform.root.gameObject;
            if (!ListOfTargetHits.Contains(hitTarget))
            {
                if (hit.TryGetComponent(out IDamgeable damgeable))
                {
                    damgeable.TakeDamage(damage, Camera.main.transform.forward);
                    ListOfTargetHits.Add(hitTarget);
                }
                
            }
        }
        
        yield return new WaitForSeconds(attackCooldown - windUpTime);
        animator.Play("Knife Standing");
        ListOfTargetHits.Clear();
        canAttack = true;

    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
