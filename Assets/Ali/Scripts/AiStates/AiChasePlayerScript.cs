using UnityEngine;
using UnityEngine.AI;

public class AiChasePlayerScript : AiState
{
    


    private AiAgent agent;

    private float timer = 0;

    public AiStateId GetId()
    {
        return AiStateId.ChasePlayer;
    }
    public void Enter(AiAgent agent)
    {
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
                 agent.navMeshAgent.destination = agent.playerPos;

                }
        
                if (timer < 0)
                {
                   agent.playerPos = Movement.instance.transform.position;

                    float sqrDistance = (agent.playerPos - agent.navMeshAgent.destination).sqrMagnitude;
                    if (sqrDistance >agent.agentConfig.maxDistance * agent.agentConfig.maxDistance)
                    {
                        agent.navMeshAgent.destination = agent.playerPos; // this is very heavy line on the cpu so we have to check if we need to use it by the 2 if statments up 
                    }
                    timer =agent.agentConfig.maxTime;
                }
               agent.animator.SetFloat("Speed", agent.navMeshAgent.velocity.magnitude);

            
        
    }

    public void Exit(AiAgent agent)
    {

    }
}
