using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class Move : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private NavMeshAgent agent;
    [SerializeField, HideInInspector]
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetFloat("Blend", agent.velocity.sqrMagnitude);
    }
}