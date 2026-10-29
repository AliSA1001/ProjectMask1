using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class AiStateMachine : MonoBehaviour
{
    public AiState[] states;
    public AiAgent agent;
    public AiStateId currentState;

    public AiStateMachine(AiAgent agent) // constract
    {
        this.agent = agent;
        int numStates = System.Enum.GetNames(typeof(AiStateId)).Length;
        states = new AiState[numStates];
    }

    public void RegisterState(AiState state)
    {
        int index = (int)state.GetId();
        states[index] = state;
    }

    public AiState GetState(AiStateId stateId)
    {
        int index = (int)stateId;
        return states[index];
    }

    public void Update()
    {
        GetState(currentState)?.Update(agent);
    }
    public void ChangeState(AiStateId newStateId)
    {
        GetState(currentState)?.Exit(agent);
        currentState = newStateId;
        GetState(currentState)?.Enter(agent);
    }

}
