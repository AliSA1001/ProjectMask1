using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class AiStateMachine : MonoBehaviour
{
    public AiState[] states;
    public AiAgent agent;

    public AiStateMachine(AiAgent agent)
    {
        this.agent = agent;
        int numStates = System.Enum.GetNames(typeof(AiStateId)).Length;
        states = new AiState[numStates];
    }
}
