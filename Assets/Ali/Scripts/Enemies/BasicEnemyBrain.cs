using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBrain : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 playerPos;
    

    [SerializeField] private float attackDistnce;
    [SerializeField] private float maxTime = 1;
    [SerializeField] private float maxDistance = 1;


    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        playerPos = Movement.instance.transform.position;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            playerPos = Movement.instance.transform.position;

            float sqrDistance = (playerPos - agent.destination).sqrMagnitude;
            if(sqrDistance > maxDistance * maxDistance)
            {
                agent.destination = playerPos; // this is very heavy line on the cpu so we have to check if we need to use it by the 2 if statments up 
            }
            timer =maxTime;
        }
        animator.SetFloat("Speed" , agent.velocity.magnitude);
            
    }
}
