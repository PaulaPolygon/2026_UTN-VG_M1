using UnityEngine;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class PeleteroPatrol : MonoBehaviour, IEnemyStrategy, IEnemyContactStrategy
{
    [SerializeField, Min(0.1f)] private float patrolSpeed = 1.5f;
    [SerializeField, Min(0f)] private float patrolDistance = 3f;
    [SerializeField] private Vector3 patrolDirection = Vector3.right;
    [SerializeField, Min(1)] private int contactDamage = 1;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    private Rigidbody body;
    private Vector3 patrolOrigin;
    private Vector3 worldPatrolDirection;
    private bool movingPositive = true;
    private float nextDamageTime;

    private void Awake()
    {
        GetComponent<CapsuleCollider>().isTrigger = true;

        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.constraints =
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        patrolOrigin = transform.position;
        worldPatrolDirection = transform.TransformDirection(patrolDirection);
        worldPatrolDirection = Vector3.ProjectOnPlane(worldPatrolDirection, Vector3.up).normalized;
        if (worldPatrolDirection.sqrMagnitude < 0.0001f)
        {
            Debug.LogError("PeleteroPatrol requires a non-zero horizontal patrol direction.", this);
            enabled = false;
        }
    }

    public void Execute()
    {
        if (patrolDistance <= 0f)
        {
            return;
        }

        Vector3 endpoint = patrolOrigin +
                           worldPatrolDirection * patrolDistance * (movingPositive ? 1f : -1f);
        Vector3 offset = endpoint - body.position;
        offset.y = 0f;
        if (offset.sqrMagnitude <= 0.01f)
        {
            movingPositive = !movingPositive;
            return;
        }

        Vector3 direction = offset.normalized;
        body.MovePosition(
            Vector3.MoveTowards(body.position, endpoint, patrolSpeed * Time.fixedDeltaTime)
        );
        body.MoveRotation(
            Quaternion.RotateTowards(
                body.rotation,
                Quaternion.LookRotation(direction),
                turnSpeed * Time.fixedDeltaTime
            )
        );
    }

    public void OnContact(Collider other)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        nextDamageTime = Time.time + damageInterval;
        player.TakeDamage(contactDamage);
    }
}
