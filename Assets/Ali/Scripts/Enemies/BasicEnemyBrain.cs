using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBrain : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 playerPos;

    [SerializeField] private float attackDistnce;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Start()
    {
        playerPos = Movement.instance.transform.position;
    }

    private void Update()
    {
        playerPos = Movement.instance.transform.position;

        agent.destination = playerPos;
            
        
    }
}
