using UnityEngine;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class ButcherChase : MonoBehaviour, IEnemyStrategy, IEnemyContactStrategy
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.1f)] private float chaseSpeed = 2f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.6f;
    [SerializeField, Min(1)] private int contactDamage = 1;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    private Rigidbody body;
    private PlayerHealth targetHealth;
    private float nextDamageTime;

    private void Awake()
    {
        GetComponent<CapsuleCollider>().isTrigger = true;

        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.constraints =
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (target == null)
        {
            Debug.LogError("ButcherChase needs the player's Transform assigned.", this);
            enabled = false;
            return;
        }

        targetHealth = target.GetComponentInParent<PlayerHealth>();
        if (targetHealth == null)
        {
            Debug.LogError("ButcherChase target needs a PlayerHealth component.", this);
            enabled = false;
        }
    }

    public void Execute()
    {
        Vector3 offset = target.position - body.position;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector3 direction = offset.normalized;
        if (offset.magnitude > stoppingDistance)
        {
            Vector3 destination = new Vector3(target.position.x, body.position.y, target.position.z);
            body.MovePosition(
                Vector3.MoveTowards(body.position, destination, chaseSpeed * Time.fixedDeltaTime)
            );
        }

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
        if (player != targetHealth)
        {
            return;
        }

        nextDamageTime = Time.time + damageInterval;
        player.TakeDamage(contactDamage);
    }
}
