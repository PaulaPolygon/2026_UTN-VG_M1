using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class Move : MonoBehaviour
{
    private static readonly int BlendParameter = Animator.StringToHash("Blend");
    private static readonly int WalkingState = Animator.StringToHash("Base Layer.Walking");
    private const float MovingThreshold = 0.1f;

    [SerializeField, HideInInspector]
    private NavMeshAgent agent;
    [SerializeField, HideInInspector]
    private Animator animator;
    private bool hasBlendParameter;
    private bool wasMoving;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == BlendParameter && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasBlendParameter = true;
                break;
            }
        }

        if (!hasBlendParameter && !animator.HasState(0, WalkingState))
        {
            Debug.LogError("Move requires a float Blend parameter or a Base Layer.Walking state in the Animator Controller.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        float speedSquared = agent.velocity.sqrMagnitude;
        if (hasBlendParameter)
        {
            animator.SetFloat(BlendParameter, speedSquared);
            return;
        }

        bool isMoving = speedSquared > MovingThreshold;
        animator.speed = isMoving ? 1f : 0f;

        if (isMoving)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (!wasMoving || state.normalizedTime >= 1f)
            {
                animator.Play(WalkingState, 0, 0f);
            }
        }

        wasMoving = isMoving;
    }
}