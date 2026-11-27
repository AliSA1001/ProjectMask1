using UnityEngine;
using UnityEngine.AI;

// NavMesh version of ICustomerMover. Needs a baked NavMesh in the scene (AI Navigation package)
[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshCustomerMover : MonoBehaviour, ICustomerMover
{
    [SerializeField] private float arriveDistance = 0.3f;
    [Tooltip("If the agent stands still this long without reaching the target we count it as arrived so nobody gets stuck forever. 0 = off")]
    [SerializeField] private float stuckTime = 2f;

    private NavMeshAgent agent;
    private bool moving;
    private float stillTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!moving) return;

        if (!agent.pathPending && agent.velocity.sqrMagnitude < 0.01f) stillTimer += Time.deltaTime;
        else stillTimer = 0f;
    }

    public bool HasArrived
    {
        get
        {
            if (!moving) return true;
            if (!agent.isOnNavMesh) return true;
            if (agent.pathPending) return false;

            float dist = Mathf.Max(arriveDistance, agent.stoppingDistance);
            if (agent.remainingDistance <= dist) return true;

            // partial path, blocked door, whatever. Don't hang forever
            return stuckTime > 0f && stillTimer >= stuckTime;
        }
    }

    public void MoveTo(Vector3 destination)
    {
        stillTimer = 0f;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} is not on the NavMesh, is it baked?", this);
            moving = false;
            return;
        }

        agent.isStopped = false;
        moving = agent.SetDestination(destination);
    }

    public void Stop()
    {
        moving = false;
        if (agent.isOnNavMesh) agent.ResetPath();
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }
}
