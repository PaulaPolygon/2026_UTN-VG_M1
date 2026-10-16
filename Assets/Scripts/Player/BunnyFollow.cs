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
    private bool targetOutsideNavMeshWarningShown;
    private bool partialPathWarningShown;
    private bool destinationErrorShown;

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
        if (!NavMesh.SamplePosition(target.position, out NavMeshHit targetHit, 2f, agent.areaMask))
        {
            if (!targetOutsideNavMeshWarningShown)
            {
                Debug.LogWarning("BunnyFollow cannot find walkable NavMesh near the target. Check that the target is on a baked NavMesh.", this);
                targetOutsideNavMeshWarningShown = true;
            }

            return;
        }

        targetOutsideNavMeshWarningShown = false;

        Vector3 flatPosition = transform.position;
        Vector3 flatTargetPosition = targetHit.position;
        flatPosition.y = 0f;
        flatTargetPosition.y = 0f;
        float distanceToTarget = Vector3.Distance(flatPosition, flatTargetPosition);

        if (distanceToTarget <= followDistance)
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if (!agent.pathPending && agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            if (!partialPathWarningShown)
            {
                Debug.LogWarning("BunnyFollow has only a partial path to the target. The baked NavMesh may have a gap or end before the target.", this);
                partialPathWarningShown = true;
            }
        }
        else if (!agent.pathPending && agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            partialPathWarningShown = false;
        }

        if (Time.time < nextDestinationUpdate)
        {
            return;
        }

        nextDestinationUpdate = Time.time + destinationUpdateInterval;
        bool destinationAccepted = agent.SetDestination(targetHit.position);
        if (!destinationAccepted && !destinationErrorShown)
        {
            Debug.LogError("BunnyFollow could not set a NavMesh destination. Check that the agent is on a compatible NavMesh.", this);
            destinationErrorShown = true;
        }
        else if (destinationAccepted)
        {
            destinationErrorShown = false;
        }
    }
}
