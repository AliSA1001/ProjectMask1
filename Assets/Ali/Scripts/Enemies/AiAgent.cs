
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AiAgent : MonoBehaviour
{
    public AiStateMachine stateMachine;
    [SerializeField] private AiStateId initialState;
    public NavMeshAgent navMeshAgent;
    public AIAgentConfig agentConfig;
    public RagDoll ragDoll;
    public SkinnedMeshRenderer mesh;
    public UIHpBar hpBar;

    private void Awake()
    {
        stateMachine = new AiStateMachine(this);
        navMeshAgent = GetComponent<NavMeshAgent>();

        ragDoll= GetComponent<RagDoll>();
        mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        hpBar = GetComponentInChildren<UIHpBar>();

    }
    private void Start()
    {
        stateMachine.RegisterState(new AiChasePlayerScript());
        stateMachine.RegisterState(new AiDeathState());
        stateMachine.ChangeState(initialState);
    }
    private void Update()
    {
        stateMachine.Update();
    }
}
