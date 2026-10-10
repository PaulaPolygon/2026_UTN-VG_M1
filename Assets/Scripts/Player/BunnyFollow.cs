using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BunnyFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float followDistance = 2f;
    [SerializeField, Min(0.05f)] private float destinationUpdateInterval = 0.2f;

    private NavMeshAgent agent;
    private float nextDestinationUpdate;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = followDistance;
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("BunnyFollow needs a target Transform assigned.", this);
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("BunnyFollow requires the bunny to start on a baked NavMesh.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= followDistance)
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if (Time.time < nextDestinationUpdate)
        {
            return;
        }

        nextDestinationUpdate = Time.time + destinationUpdateInterval;
        agent.SetDestination(target.position);
    }
}
