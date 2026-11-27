using UnityEngine;
using UnityEngine.AI;

public class DebugNavMeshAgent : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private bool velocity;
    [SerializeField] private bool desiredVelocity;
    [SerializeField] private bool path;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnDrawGizmos()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (velocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position,transform.position + agent.velocity);
        }
        if (desiredVelocity)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + agent.desiredVelocity);
        }
        if (path)
        {
            Gizmos.color = Color.black;
            var agentPath = agent.path;
            Vector3 prevCorner = transform.position;
            foreach(var corner in agentPath.corners)
            {
                Gizmos.DrawLine (prevCorner, corner);
                Gizmos.DrawSphere(corner, 0.1f);
                prevCorner = corner;
            }
        }
    }
}
