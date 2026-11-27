using UnityEngine;

public class AiDeathState : AiState
{
    public Vector3 direction;
    public AiStateId GetId()
    {
        return AiStateId.Death;
    }
    public void Enter(AiAgent agent)
    {

       agent. ragDoll.ActivateRagdoll();
       direction.y = 1;
       agent.ragDoll.ApplyForce(direction * agent.agentConfig.dieForce);
       agent.hpBar.gameObject.SetActive(false);
    }

    public void Update(AiAgent agent)
    {

        


    }

    public void Exit(AiAgent agent)
    {

    }

}
