using UnityEngine;
using UnityEngine.AI;

public class AiChasePlayerScript : AiState
{
    
    private Vector3 playerPos;


    private AiAgent agent;

    private float timer = 0;

    public AiStateId GetId()
    {
        return AiStateId.ChasePlayer;
    }
    public void Enter(AiAgent agent)
    {
    
        playerPos = Movement.instance.transform.position;

    }

    public void Update(AiAgent agent)
    {
        
            if (!agent.enabled)
            {
                return;
            }
                timer -= Time.deltaTime;
        
                if(agent.navMeshAgent.hasPath)
                {
                 agent.navMeshAgent.destination = playerPos;

                }
        
                if (timer < 0)
                {
                    playerPos = Movement.instance.transform.position;

                    float sqrDistance = (playerPos - agent.navMeshAgent.destination).sqrMagnitude;
                    if (sqrDistance >agent.agentConfig.maxDistance * agent.agentConfig.maxDistance)
                    {
                        agent.navMeshAgent.destination = playerPos; // this is very heavy line on the cpu so we have to check if we need to use it by the 2 if statments up 
                    }
                    timer =agent.agentConfig.maxTime;
                }
              //  animator.SetFloat("Speed", agent.navMeshAgent.velocity.magnitude);

            
        
    }

    public void Exit(AiAgent agent)
    {

    }
}
