
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
    public Animator animator;
    public Vector3 playerPos;


    private void Awake()
    {
        stateMachine = new AiStateMachine(this);
        navMeshAgent = GetComponent<NavMeshAgent>();

        ragDoll= GetComponent<RagDoll>();
        mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        hpBar = GetComponentInChildren<UIHpBar>();
        animator = GetComponent<Animator>();

    }
    private void Start()
    {

        playerPos = Movement.instance.transform.position;

        stateMachine.RegisterState(new AiChasePlayerScript());
        stateMachine.RegisterState(new AiDeathState());
        stateMachine.RegisterState(new AiIdelState());
        stateMachine.RegisterState(new AiFindWwaponState());
        stateMachine.ChangeState(initialState);


    }
    private void Update()
    {
        playerPos = Movement.instance.transform.position;


        stateMachine.Update();
    }
}
