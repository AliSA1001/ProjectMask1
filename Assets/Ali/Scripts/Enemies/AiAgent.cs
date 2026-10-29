
using Unity.VisualScripting;
using UnityEngine;

public class AiAgent : MonoBehaviour
{
    private AiStateMachine stateMachine;
    [SerializeField] private AiStateId initialState;


    private void Start()
    {
        stateMachine = new AiStateMachine(this);
        stateMachine.ChangeState(initialState);
    }
    private void Update()
    {
        stateMachine.Update();
    }
}
