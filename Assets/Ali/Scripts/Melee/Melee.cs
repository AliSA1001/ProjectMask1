using NUnit.Framework;
using System;
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

    [Header("2nd attack")]
    [SerializeField] private float windUpTime2 = 0.34f;
    [SerializeField] private float attackCooldown2 = 1f;
    private bool can2ndAttack = false;
    private bool isDoing2ndAttack = false;

    [Header("combo")]
    [SerializeField] private float comboThreshold = 0.15f;


    private Animator animator;
    private bool canAttack = true;
    public static Action OnDurabilityLose;




    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        canAttack = true;
        animator.Play("Knife Standing");
    }

    private void Update()
    {
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && canAttack)
        {
            StartCoroutine(Attack());
        }
        else if (context.performed && can2ndAttack)
        {
            isDoing2ndAttack = true;
        }
    }
    private void DurabilityLose()
    {
        OnDurabilityLose?.Invoke();
    }
    IEnumerator Attack()
    {
        canAttack = false;
        isDoing2ndAttack = false ;
        animator.Play("Knife_Attack");

        yield return new WaitForSeconds(windUpTime);

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, hitLayer);

        List<GameObject> ListOfTargetHits = new List<GameObject>();

        foreach (Collider hit in hits)
        {
            GameObject hitTarget = hit.transform.root.gameObject;
            if (!ListOfTargetHits.Contains(hitTarget))
            {
                if (hit.TryGetComponent(out IDamgeable damgeable))
                {
                    damgeable.TakeDamage(damage, Camera.main.transform.forward);
                    ListOfTargetHits.Add(hitTarget);
                    DurabilityLose();
                }

            }
        }

        yield return new WaitForSeconds(attackCooldown - windUpTime);
        float combotimer = 0;
        can2ndAttack = true;
        while(combotimer < comboThreshold)
        {
            if(isDoing2ndAttack)
            {
                StartCoroutine(Attack2()); 
                yield break;
            }
            combotimer += Time.deltaTime;
            yield return null;
        }

        animator.Play("Knife Standing");
        ListOfTargetHits.Clear();
        canAttack = true;
        can2ndAttack = false;

    }

    IEnumerator Attack2()
    {
        
        can2ndAttack = false;
        animator.Play("Knife_Attack2");

        yield return new WaitForSeconds(windUpTime2);

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, hitLayer);

        List<GameObject> ListOfTargetHits = new List<GameObject>();
        foreach (Collider hit in hits)
        {
           
            GameObject hitTarget = hit.transform.root.gameObject;
            if (!ListOfTargetHits.Contains(hitTarget))
            {
                if (hit.TryGetComponent(out IDamgeable damgeable))
                {
                    damgeable.TakeDamage(damage, Camera.main.transform.forward);
                    ListOfTargetHits.Add(hitTarget);
                    DurabilityLose();
                }

            }
        }
        yield return new WaitForSeconds(attackCooldown2 - windUpTime2);
       
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
